## Configuration

`appsettings.json` contains the setting location:

```json
"OMDb": {
  "ApiKey": "",
  "BaseUrl": "https://www.omdbapi.com"
}
```

For development, put the real key in user secrets or an environment variable instead of committing it.

```text
OMDB__APIKEY=YOUR_OMDB_API_KEY
OMDB__BASEURL=https://www.omdbapi.com
```