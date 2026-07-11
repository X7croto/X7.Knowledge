# Error

Type: IntegrityValidationContext
Return: void

## Parameters

- string code
- string message
- string? location

## Calls

- IntegrityValidationContext.AddIssue

## Called By

- MethodScopeConsistencyRule.Execute
- IntegrityValidator.ExecuteRule
- IntegrityValidationRule.Validate
- DuplicateMemberReferenceRule.Validate
- BrokenReferenceRule.ValidateCalls
- BrokenReferenceRule.ValidateFields
- BrokenReferenceRule.ValidateMethods
- BrokenReferenceRule.ValidateProperties
- BrokenReferenceRule.ValidateTypes
