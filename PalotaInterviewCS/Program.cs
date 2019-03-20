using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PalotaInterviewCS
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private const string countriesEndpoint = "https://restcountries.eu/rest/v2/all";

        static void Main(string[] args)
        {
            List<Country> countries = GetCountries(countriesEndpoint).GetAwaiter().GetResult();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Palota Interview: Country Facts");
            Console.WriteLine();
            Console.ResetColor();

            Random rnd = new Random(); // random to populate fake answer - you can remove this once you use real values

            //TODO use data operations and data structures to optimally find the correct value (N.B. be aware of null values)

            /*
             * HINT: Sort the list in descending order to find South Africa's place in terms of gini coefficients
             * `Country.Gini` is the relevant field to use here           
             */

            var giniSortedList = countries.Where(o => o.Gini != null).OrderBy(o => o.Gini).ToList();
            var southAfrica = countries.FirstOrDefault(o => o.Name == "South Africa");
            int southAfricanGiniPlace = giniSortedList.IndexOf(southAfrica);
            Console.WriteLine($"1. South Africa's Gini coefficient is the {GetOrdinal(southAfricanGiniPlace)} highest");

            /*
             * HINT: Sort the list in ascending order or just find the Country with the minimum gini coeficient          
             * use `Country.Gini` for the ordering then return the relevant country's name `Country.Name`
             */
            string lowestGiniCountry = giniSortedList.FirstOrDefault()?.Name;
            Console.WriteLine($"2. {lowestGiniCountry} has the lowest Gini Coefficient");

            /*
             * HINT: Group by regions (`Country.Region`), then count the number of unique timezones that the countries in each region span
             * Once you have done the grouping, find the group `Region` with the most timezones and return it's name and the number of unique timezones found          
             */
            var groupedCountriesByReligion = countries.GroupBy(o => o.Region).ToList();
            string regionWithMostTimezones = null;
            int amountOfTimezonesInRegion = 0;

            foreach (var region in groupedCountriesByReligion.ToList())
            {
                var regionTimezones = new List<string>();

                foreach (var countryTimezones in region.Select(o => o.Timezones.ToList()).ToList())
                {
                    regionTimezones.AddRange(countryTimezones);
                }

                if (regionTimezones.Distinct().ToList().Count > amountOfTimezonesInRegion)
                {
                    amountOfTimezonesInRegion = regionTimezones.Distinct().ToList().Count;
                    regionWithMostTimezones = region.Key.ToString();
                }
            }

            Console.WriteLine($"3. {regionWithMostTimezones} is the region that spans most timezones at {amountOfTimezonesInRegion} timezones");

            /*
             * HINT: Count occurances of each currency in all countries (check `Country.Currencies`)
             * Find the name of the currency with most occurances and return it's name (`Currency.Name`) also return the number of occurances found for that currency          
             */
            var currencies = new Dictionary<string, int>();
            var languages = new Dictionary<string, int>();
            var countryPopulationInfo = new Dictionary<string, int[]>();
            var countryDensityInfo = new Dictionary<string, double>();

            foreach (var country in countries)
            {
                //This is for getting currencies information
                foreach (var currency in country.Currencies)
                {
                    if (!string.IsNullOrEmpty(currency.Name))
                        if (!currencies.ContainsKey(currency.Name))
                        {
                            currencies.Add(currency.Name, 1);
                        }
                        else
                        {
                            currencies[currency.Name] += 1;
                        }
                }

                //This is for languages
                foreach (var language in country.Languages)
                {
                    if (!string.IsNullOrEmpty(language.Name))
                        if (!languages.ContainsKey(language.Name))
                        {
                            languages.Add(language.Name, 1);
                        }
                        else
                        {
                            languages[language.Name] += 1;
                        }
                }

                //This is for population and bordering countries
                if (country.Borders.Length > 0)
                {
                    var population = country.Population;

                    if (!countryPopulationInfo.ContainsKey(country.Name))
                    {
                        foreach (var borderCountry in country.Borders)
                        {
                            population += countries.FirstOrDefault(o => o.Alpha3Code == borderCountry).Population;
                        }
                    }

                    countryPopulationInfo.Add(country.Name, new int[] { (int)population, country.Borders.Length });
                }

                //this is for density
                if (country.Area != null)
                    if (!countryDensityInfo.ContainsKey(country.Name))
                    {
                        var density = country.Population / country.Area;
                        countryDensityInfo.Add(country.Name, (double)density);
                    }
            }

            var popularCurrency = currencies.OrderBy(o => o.Value).LastOrDefault();
            string mostPopularCurrency = popularCurrency.Key;
            int numCountriesUsedByCurrency = popularCurrency.Value;
            Console.WriteLine($"4. {mostPopularCurrency} is the most popular currency and is used in {numCountriesUsedByCurrency} countries");

            /*
             * HINT: Count the number of occurances of each language (`Country.Languages`) and sort then in descending occurances count (i.e. most populat first)
             * Once done return the names of the top three languages (`Language.Name`)
             */
            var languagesArr = languages.OrderByDescending(o => o.Value).ToArray();
            string[] mostPopularLanguages = { languagesArr[0].Key, languagesArr[1].Key, languagesArr[2].Key };
            Console.WriteLine($"5. The top three popular languages are {mostPopularLanguages[0]}, {mostPopularLanguages[1]} and {mostPopularLanguages[2]}");

            /*
             * HINT: Each country has an array of Bordering countries `Country.Borders`, The array has a list of alpha3 codes of each bordering country `Country.alpha3Code`
             * Sum up the population of each country (`Country.Population`) along with all of its bordering countries's population. Sort this list according to the combined population descending
             * Find the country with the highest combined (with bordering countries) population the return that country's name (`Country.Name`), the number of it's Bordering countries (`Country.Borders.length`) and the combined population
             * Be wary of null values           
             */
            var countryWithMostPoplulation = countryPopulationInfo.OrderByDescending(o => o.Value[0]).ToList().FirstOrDefault();

            string countryWithBorderingCountries = countryWithMostPoplulation.Key;
            int numberOfBorderingCountries = countryWithMostPoplulation.Value[1];
            int combinedPopulation = countryWithMostPoplulation.Value[0];
            Console.WriteLine($"6. {countryWithBorderingCountries} and it's {numberOfBorderingCountries} bordering countries has the highest combined population of {combinedPopulation}");

            /*
             * HINT: Population density is calculated as (population size)/area, i.e. `Country.Population/Country.Area`
             * Calculate the population density of each country and sort by that value to find the lowest density
             * Return the name of that country (`Country.Name`) and its calculated density.
             * Be wary of null values when doing calculations           
             */

            var lowPopDensityCountry = countryDensityInfo.OrderBy(o => o.Value).FirstOrDefault();
            string lowPopDensityName = lowPopDensityCountry.Key;
            double lowPopDensity = lowPopDensityCountry.Value;
            Console.WriteLine($"7. {lowPopDensityName} has the lowest population density of {lowPopDensity}");

            /*
             * HINT: Population density is calculated as (population size)/area, i.e. `Country.Population/Country.Area`
             * Calculate the population density of each country and sort by that value to find the highest density
             * Return the name of that country (`Country.Name`) and its calculated density.
             * Be wary of any null values when doing calculations. Consider reusing work from above related question           
             */

            var highPopDensityCountry = countryDensityInfo.OrderBy(o => o.Value).LastOrDefault();
            string highPopDensityName = highPopDensityCountry.Key;
            double highPopDensity = highPopDensityCountry.Value;
            Console.WriteLine($"8. {highPopDensityName} has the highest population density of {highPopDensity}");

            /*
             * HINT: Group by subregion `Country.Subregion` and sum up the area (`Country.Area`) of all countries per subregion
             * Sort the subregions by the combined area to find the maximum (or just find the maximum)
             * Return the name of the subregion
             * Be wary of any null values when summing up the area           
             */

            var countriesGoupedBySubregion = countries.Select(o => new { o.Subregion, o.Area }).GroupBy(o => o.Subregion).ToDictionary(o => o.Key, o => o.Sum(s => s.Area)).OrderBy(o => o.Value).ToList();
            string largestAreaSubregion = countriesGoupedBySubregion.LastOrDefault().Key;
            Console.WriteLine($"9. {largestAreaSubregion} is the subregion that covers the most area");

            /*
             * HINT: Group by regional blocks (`Country.RegionalBlocks`). For each regional block, average out the gini coefficient (`Country.Gini`) of all member countries
             * Sort the regional blocks by the average country gini coefficient to find the lowest (or find the lowest without sorting)
             * Return the name of the regional block (`RegionalBloc.Name`) along with the calculated average gini coefficient
             */

            //Im not sure whether i should group by the array of block or by each regional block, but i think it makes no sense to group by an array
            var regionalBlocks = countries.Select(o => o.RegionalBlocs.Select(r => r.Name).ToList()).ToList();
            var regionalBlockList = new List<string>();
            var countriesGoupedByRegionalBlocks = new Dictionary<string, List<Country>>();

            foreach (var regionalBlock in regionalBlocks.Where(o => o != null && o.Count > 0).ToList())
            {
                regionalBlockList.AddRange(regionalBlock);
            }

            string mostEqualRegionalBlock = null;
            double lowestRegionalBlockGini = (double)countries.Max(o => o.Gini);

            foreach (var regionalBlock in regionalBlockList.Distinct().ToList())
            {
                var avarage = countries.Where(o => o.RegionalBlocs.ToList().Any(r => r.Name == regionalBlock)).Average(o => o.Gini);

                if (avarage < lowestRegionalBlockGini)
                {
                    lowestRegionalBlockGini = (double)avarage;
                    mostEqualRegionalBlock = regionalBlock;
                }
            }

            //var countriesGoupedByRegionalBlocks = countries.Select(o => new { o.RegionalBlocs, o.Gini }).GroupBy(o => o.RegionalBlocs).ToDictionary(o => o.Key, o => o.Average(s => s.Gini)).ToList();

            Console.WriteLine($"10. {mostEqualRegionalBlock} is the regional block with the lowest average Gini coefficient of {lowestRegionalBlockGini}");
            Console.ReadLine();
        }

        /// <summary>
        /// Gets the countries from a specified endpiny
        /// </summary>
        /// <returns>The countries.</returns>
        /// <param name="path">Path endpoint for the API.</param>
        static async Task<List<Country>> GetCountries(string path)
        {
            //TODO get data from endpoint and convert it to a typed arr
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(path);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<List<Country>>();
            }

            return null;
        }

        /// <summary>
        /// Gets the ordinal value of a number (e.g. 1 to 1st)
        /// </summary>
        /// <returns>The ordinal.</returns>
        /// <param name="num">Number.</param>
        public static string GetOrdinal(int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }

        }
    }
}
