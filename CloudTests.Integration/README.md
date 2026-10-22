# Integration Tests

This project contains integration tests that test the full flow with real Firestore.

## Setup

### Option 1: Use Firestore Emulator (Recommended for CI/CD)

1. Install Firebase CLI: `npm install -g firebase-tools`
2. Start emulator: `firebase emulators:start --only firestore`
3. Set environment variable: `FIRESTORE_EMULATOR_HOST=localhost:8080`

### Option 2: Use Test Firestore Project

1. Create a separate Firestore project for testing
2. Set `GOOGLE_APPLICATION_CREDENTIALS` to test service account
3. Update project ID in appsettings or environment variables

## Running Tests

```bash
# Run all integration tests
dotnet test --filter "TestCategory=Integration"

# Run only unit tests (exclude integration)
dotnet test --filter "TestCategory!=Integration"
```

## Test Structure

- **IntegrationTestBase**: Base class that sets up real services
- **ItemDropIntegrationTests**: Example integration tests
- Tests marked with `[TestCategory("Integration")]` for filtering

## Notes

- Integration tests are slower and require Firestore connection
- Use test data cleanup in `[TestCleanup]` to avoid test pollution
- Consider using a separate test database/project
