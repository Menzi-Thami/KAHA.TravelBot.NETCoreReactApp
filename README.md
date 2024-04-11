# KAHA Programming Assignment - ASP.NET Core with React User Interface (v2.1.231124)

At Kaha, we love to travel. 

We've put together a simple program that helps us envision our next vacation destination, equipped with all the necessary details for your journey.

### Instructions
The purpose of this assignment is to evaluate your coding, debugging, and API integration skills.
  - **As a minimum requirement, the project should compile and run.**
  - Your solution should be clean, well-organized, and thoroughly tested. If you are unsure of anything, state your assumptions in comments as you proceed with the assignment.
  - You may make use of any external libraries, such as Newtonsoft.Json or System.Text.Json for proper JSON parsing and deserialization for example.
  - Refactor and improve the code as you see fit.
  > **Starter project**: We've provided some boilerplate code as a starting point. The project uses the Visual Studio 2022 Project Template: ASP.NET Core with React.js.

### Submission
  - When you are done, make a screen recording of yourself demonstrating the functionality you’ve worked on and explaining what you have done. It should be between 5 and 10 minutes.
  - At the end of the recording, give us your thoughts on the assignment and how long it took you to complete.
  - We can recommend using [Vimeo](https://vimeo.com) or [Zoom Clips](https://support.zoom.com/hc/en/article?id=zm_kb&sysparm_article=KB0057723) to make the recording.
  - Share the code with us using WeTransfer, Google Drive, or some other link-based sharing (zip files don’t travel well via email ??)
  - Send the link to the recording, along with a link to your code through to us on <chat@kaha.co.za>

### Your assignment is to complete the following 5 tasks in the given .NET Core and React project:
  1. **Fetching Country Data: (Working)**
     - Refactor the `TravelBotService.GetAllCountries` function, which currently fetches a list of all countries from https://restcountries.com/v3.1/all, to improve the code quality (note that it's intentionally suboptimal).
     - Ensure refactoring does not break the function, as later tasks depend on it.

  2. **Top 5 Country Selection:**
     - Complete the `TravelBotService.GetTopFiveCountries` method to return the top 5 most populous countries in the Southern Hemisphere.
     - Complete the `CountriesController.GetTopFive` method to return the top 5 countries, providing essential details (Name, Capital, Population, Latitude, Longitude) in JSON format.

  3. **Sunrise and Sunset Times:**
     - Implement `GetSunriseSunsetTimes` to obtain the sunrise and sunset times for any capital city.
     - Use https://sunrise-sunset.org/api for the data and format the results for readability.

  4. **Travel Bot Summary:**
     - Complete the TravelBotService.GetCountrySummary method, returning an interesting country summary, for a given Country Name.
     - This can consist of some key information that would be interesting for a potential traveller.
     - Include the Sunrise and Sunset times for the capital city of the given country.
     - Some key stats that we would like to see: Total number of official languages, the side of the road inhabitants drive on.
     - Using the `TravelBotService.GetCountrySummary` method, complete the `CountriesController.GetSummary` method to return the data in JSON format.

  5. **User Interface:**
     - Add a "Travel Bot" section to the ClientApp, similar to the "Fetch Data" section.
     - Display a table listing the top 5 countries with their basic details, populated via the "GetTopFive" endpoint.
     - Implement a feature to view full country information on click, using data from the "GetSummary" endpoint.

> **NOTE**: The following questions are for senior-level candidates, but feel free to tackle them as bonus challenges.

  6. **BONUS:** Random Country Selection:
     - Correct a subtle bug in the function selecting a random Southern Hemisphere country.
     - Complete the GetRandomCountry API endpoint to fetch the Country Summary for a randomly chosen country.
     - Include a "Surprise Me" button in the UI that displays details of a random country using the new API.

  7. **BONUS:** CountrySummary.DistanceFromKAHA:
     - KAHA Office location: <https://www.google.com/maps/place/Kaha/@-33.9759679,18.4566283,17z/data=!3m1!4b1!4m6!3m5!1s0x1dcc43998511fac3:0x4d7e223b5879a20a!8m2!3d-33.9759724!4d18.4592032!16s%2Fg%2F11h5l47g67?entry=ttu>
     - Create a method to calculate the distance from a country's capital to the KAHA offices.
     - Add this as a property to the CountrySummary model.
     - Show this distance in the UI's Country Summary.

  8. **BONUS:** Backend Enhancements & Refactoring
     - Implement the TravelBotService as a Singleton service.
     - Cache countries in a static variable within the service, resorting to API calls only when necessary.
     - Integrate TravelBotService through dependency injection.

  9. **BONUS**: Frontend Enhancements:
     - Here's your chance to show off frontend skills.
     - Feel free to make any improvements that will enhance UI, UX, maintainability, or scalability.

Happy coding!