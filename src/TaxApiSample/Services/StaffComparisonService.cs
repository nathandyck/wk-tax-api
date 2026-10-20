using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ExcelDataReader;

namespace TaxApiSample.Services;

public sealed class StaffComparisonService(StaffApiClient staffApiClient)
{
    private static readonly IReadOnlyDictionary<string, string> WorkbookFieldMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["firstname"] = "FirstName",
            ["lastname"] = "LastName",
            ["middlename"] = "MiddleName",
            ["suffix"] = "Suffix",
            ["title"] = "Honorific",
            ["businessunit"] = "BusinessUnitName",
            ["department"] = "FirmDepartmentName",
            ["homeoffice"] = "HomeOfficeName",
            ["reportname"] = "ReportName",
            ["reportingmanager"] = "ReportingManagerName",
            ["staffid"] = "StaffId",
            ["status"] = "StaffStatusCd",
            ["systememail"] = "SystemEmailAddress"
        };

    public async Task<StaffComparisonResult> CompareAsync(
        string workbookPath,
        string integratorKey,
        string? securityToken,
        string? authorizationToken,
        CancellationToken cancellationToken)
    {
        var workbookStaff = ReadWorkbook(workbookPath);
        var differences = new List<StaffDifference>();
        var missingStaff = new List<string>();
        var unchangedStaffCount = 0;

        foreach (var spreadsheetStaff in workbookStaff)
        {
            var axcessStaff = await staffApiClient.GetStaffAsync(
                spreadsheetStaff.StaffId,
                integratorKey,
                securityToken,
                authorizationToken,
                cancellationToken);

            if (axcessStaff is null)
            {
                missingStaff.Add(spreadsheetStaff.StaffId);
                continue;
            }

            var changes = new List<StaffFieldChange>();
            foreach (var (field, proposedValue) in spreadsheetStaff.Values)
            {
                if (!axcessStaff.Value.TryGetProperty(field, out var currentValue))
                {
                    throw new InvalidOperationException(
                        $"Staff lookup for StaffId '{spreadsheetStaff.StaffId}' did not include field '{field}'.");
                }

                var rawCurrentValue = GetTextValue(currentValue);
                var currentText = TranslateAxcessValue(field, rawCurrentValue);
                if (!string.Equals(currentText, proposedValue, StringComparison.OrdinalIgnoreCase))
                {
                    changes.Add(new StaffFieldChange(field, currentText, proposedValue));
                }
            }

            if (changes.Count == 0)
            {
                unchangedStaffCount++;
                continue;
            }

            var staffName = ReadStaffName(axcessStaff.Value);
            differences.Add(new StaffDifference(spreadsheetStaff.StaffId, staffName, changes));
        }

        return new StaffComparisonResult(
            workbookStaff.Count,
            differences,
            missingStaff,
            unchangedStaffCount);
    }

    private static List<WorkbookStaff> ReadWorkbook(string workbookPath)
    {
        if (!File.Exists(workbookPath))
        {
            throw new FileNotFoundException($"Staff workbook not found: '{workbookPath}'.", workbookPath);
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var stream = File.OpenRead(workbookPath);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        if (!reader.Read())
        {
            throw new InvalidOperationException($"Staff workbook '{workbookPath}' contains no rows.");
        }

        var fieldColumns = new Dictionary<int, string>();
        var staffIdColumn = -1;
        for (var column = 0; column < reader.FieldCount; column++)
        {
            var header = reader.GetValue(column)?.ToString()?.Trim();
            if (string.IsNullOrEmpty(header))
            {
                continue;
            }

            var normalizedHeader = NormalizeHeader(header);
            if (!WorkbookFieldMap.TryGetValue(normalizedHeader, out var apiField))
            {
                continue;
            }

            if (fieldColumns.Values.Contains(apiField, StringComparer.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Staff workbook contains more than one column mapped to '{apiField}'.");
            }

            fieldColumns.Add(column, apiField);
            if (apiField == "StaffId")
            {
                staffIdColumn = column;
            }
        }

        if (staffIdColumn < 0)
        {
            throw new InvalidOperationException("Staff workbook must contain a 'Staff ID' column.");
        }

        if (fieldColumns.Count == 1)
        {
            throw new InvalidOperationException("Staff workbook contains no comparable staff fields.");
        }

        var staff = new List<WorkbookStaff>();
        var staffIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var rowNumber = 1;
        while (reader.Read())
        {
            rowNumber++;
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var (column, field) in fieldColumns)
            {
                var cellValue = GetWorkbookCellValue(reader.GetValue(column));
                if (string.IsNullOrWhiteSpace(cellValue))
                {
                    continue;
                }

                if (field != "StaffId")
                {
                    values.Add(field, cellValue);
                }
            }

            var staffId = GetWorkbookCellValue(reader.GetValue(staffIdColumn));
            if (string.IsNullOrWhiteSpace(staffId))
            {
                if (values.Count == 0)
                {
                    continue;
                }

                throw new InvalidOperationException(
                    $"Staff workbook row {rowNumber} has values but no Staff ID.");
            }

            if (!staffIds.Add(staffId))
            {
                throw new InvalidOperationException(
                    $"Staff workbook contains duplicate Staff ID '{staffId}' (row {rowNumber}).");
            }

            staff.Add(new WorkbookStaff(staffId, values));
        }

        if (staff.Count == 0)
        {
            throw new InvalidOperationException("Staff workbook contains no staff rows.");
        }

        return staff;
    }

    private static string? TranslateAxcessValue(string field, string? value) =>
        field == "StaffStatusCd" && string.Equals(value?.Trim(), "A", StringComparison.OrdinalIgnoreCase)
            ? "Active"
            : value;

    private static string NormalizeHeader(string header) =>
        new(header.Where(char.IsLetterOrDigit).ToArray());

    private static string GetWorkbookCellValue(object? value) =>
        value switch
        {
            null => string.Empty,
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty,
            _ => value.ToString()?.Trim() ?? string.Empty
        };

    private static string? GetTextValue(JsonElement value) =>
        value.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.String => value.GetString(),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            _ => value.ToString()
        };

    private static string ReadStaffName(JsonElement staff)
    {
        var firstName = staff.TryGetProperty("FirstName", out var first)
            ? GetTextValue(first)
            : null;
        var lastName = staff.TryGetProperty("LastName", out var last)
            ? GetTextValue(last)
            : null;
        return string.Join(' ', new[] { firstName, lastName }.Where(name => !string.IsNullOrWhiteSpace(name)));
    }

    private sealed record WorkbookStaff(string StaffId, IReadOnlyDictionary<string, string> Values);
}

public sealed record StaffComparisonResult(
    [property: JsonPropertyName("SpreadsheetStaffCount")] int SpreadsheetStaffCount,
    [property: JsonPropertyName("Differences")] IReadOnlyList<StaffDifference> Differences,
    [property: JsonPropertyName("StaffNotFoundInAxcess")] IReadOnlyList<string> StaffNotFoundInAxcess,
    [property: JsonPropertyName("UnchangedStaffCount")] int UnchangedStaffCount);

public sealed record StaffDifference(
    [property: JsonPropertyName("StaffId")] string StaffId,
    [property: JsonPropertyName("StaffName")] string StaffName,
    [property: JsonPropertyName("Changes")] IReadOnlyList<StaffFieldChange> Changes);

public sealed record StaffFieldChange(
    [property: JsonPropertyName("Field")] string Field,
    [property: JsonPropertyName("AxcessValue")] string? AxcessValue,
    [property: JsonPropertyName("SpreadsheetValue")] string SpreadsheetValue);
