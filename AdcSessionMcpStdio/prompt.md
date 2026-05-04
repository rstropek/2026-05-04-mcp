Before moving on, read the first 100 lines of session.json. We will use this data in our MCP server. Make sure you create proper C# records that reflect the data structure. Also, add a separate file with helper functions to deserilaize the data.

Generate a MCP Server with the following logic:

* Get a list of all distinct themes
* Get a list of sessions with the following optional filters:
  * Theme (includes)
  * Speaker (name includes)
  * Title (includes)
  * Optionally sorted by one or more of the following:
    * Start date + time, ascending
    * Title, ascending
  * After the sorts mentioned above, always sort based on ID (ascending)
  * Paging (5 sessions per page)
  * Returns all available session data for each session that matches the filters

Please ensure that sessions.json is copied into the build target directory.
