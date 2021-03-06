# ZIP code info viewer

Create a .NET Web API that takes ZIP-code as a parameter, then outputs city name, current temperature, time zone, and general elevation at the location with a user-friendly message. For example, “At the location $CITY_NAME, the temperature is $TEMPERATURE, and the timezone is $TIMEZONE”. Include documentation with any necessary build instructions and be prepared to discuss your approach.

- Use the Open WeatherMap current weather API to retrieve the current temperature and city name. You will be required to sign up for a free API key.
- Use the Google Time Zone API to get the current timezone for a location. You will again need to register a “project” and sign up for a free API key * with Google.

UI (Optional)
In addition to the required assignment above, write a modern JS-based SPA front-end that allows a user to input a zipcode, then output the response-message to the page, preferably utilizing the tool you created above.

Used:
- net core 3.1
- angular 8
- Polly 7
- MSTest, Moq

How to run:
- update appsettings.json in ZCI project.
- open ZCI.sln file, press F5. (better to Launch project, profile ZCI)

Note - the project has never been launched with real keys, only tests.
