# Unity Cloud Save Integration

This implementation allows your server-side .NET code to directly communicate with the Unity Cloud Save service via REST API.

## Setup Instructions

### 1. Authentication

The Cloud Save REST API requires authentication. Set the following environment variables:

```
UNITY_PROJECT_ID=your-unity-project-id
UNITY_ACCESS_TOKEN=your-access-token
```

To obtain these values:

1. **Project ID**: Find this in your Unity Dashboard under Project Settings
2. **Access Token**: You'll need to generate this. There are two main approaches:
   - For development: Generate a personal access token from the Unity Dashboard
   - For production: Set up a service account with proper permissions

### 2. Usage

The `CloudSaveManager` provides three main methods:

```csharp
// Save data for a player
await cloudSaveManager.SavePlayerData(playerId, dataDict);

// Load data for a player (all or specific keys)
var data = await cloudSaveManager.LoadPlayerData(playerId, keys);

// Delete specific player data
await cloudSaveManager.DeletePlayerData(playerId, key);
```

### 3. Integration with ArmoryService

The ArmoryService already uses the CloudSaveManager to save merged items to both Firestore and Unity Cloud Save, providing redundancy and cross-platform access.

## API Endpoints Used

This implementation uses the following Unity Cloud Save REST API endpoints:

- `PUT /v1/projects/{projectId}/players/{playerId}/data` - Save player data
- `GET /v1/projects/{projectId}/players/{playerId}/data` - Load player data
- `DELETE /v1/projects/{projectId}/players/{playerId}/data/{key}` - Delete player data

## Troubleshooting

If you encounter errors with the Cloud Save integration:

1. Check that your environment variables are set correctly
2. Verify that the access token has the correct permissions
3. Look at the logs which include detailed error information
4. If you get 401 Unauthorized errors, the token may have expired - implement proper token refresh

## References

- [Unity Cloud Save Documentation](https://docs.unity.com/ugs/manual/cloud-save/manual)
- [Unity Cloud Save REST API](https://docs.unity.com/ugs/manual/cloud-save/manual/tutorials/rest-api) 