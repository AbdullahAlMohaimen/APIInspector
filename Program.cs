using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Net.Http;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Security.Policy;
using System.Data;
using System.Globalization;

namespace APIInspector
{
	public class Program
	{
		static string baseUrl = "https://localhost:44398/api/";
		static string[] endpointUrls;
		static int apiNo = 0;
		static int endpointIndex;
		static string endpointUrl;
		static string username;
		static string password;
		static string jsonContent = "";
		static string userToken = "";
		static DateTime fromDate;
		static DateTime toDate;

		static async Task Main(string[] args)
		{
			#region Set Api Collection
			endpointUrls = new string[] {
				"BATCanteen/GetAllActiveEmployeeInformation",
				"BATCanteen/GetAttendanceInformation",
				"BATCanteen/GetOvertimeInformation",
				"BATCanteen/GetShiftGroupMapping",
				"BATCanteen/GetRelayMapping",
				"BATCanteen/GetUpdatedEmployeesInformation",
				"BATCanteen/GetNewCreatedEmployeesInformation",
				"BATCanteen/GetUpdatedOvertimeInformation",
			};
			#endregion

			#region Authorize Section
			await Authorization();
			#endregion

			Console.ReadKey();
		}

		#region Authorization
		public static async Task Authorization()
		{
			using (var httpClient = new HttpClient())
			{

				Console.WriteLine("Authorization Section:");
				Console.Write("Enter UserName: ");
				username = Console.ReadLine();
				Console.Write("Enter Password: ");
				password = GetHiddenConsoleInput();

				var loginData = new { UserName = username, Password = password };
				jsonContent = JsonConvert.SerializeObject(loginData);
				var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
				HttpResponseMessage loginResponse = await httpClient.PostAsync(baseUrl + "Authentication/Login", content);
				if (loginResponse.IsSuccessStatusCode)
				{
					userToken = await loginResponse.Content.ReadAsStringAsync();
					Console.WriteLine("\nSuccessfully Authorized.");
					Console.WriteLine("Token : " + userToken);
				}
				else if (loginResponse.StatusCode == HttpStatusCode.Unauthorized)
				{
					string responseContent = await loginResponse.Content.ReadAsStringAsync();
					dynamic responseData = JsonConvert.DeserializeObject(responseContent);
					string errorMessage = responseData.message;
					Console.WriteLine($"Error: {errorMessage}");
					return;
				}
				else
				{
					Console.WriteLine($"Error: {loginResponse.StatusCode}");
					return;
				}
			}
			await GetApiCollection();
		}
		#endregion

		#region Hide Console Input
		static string GetHiddenConsoleInput()
		{
			string input = "";
			ConsoleKeyInfo key;
			do
			{
				key = Console.ReadKey(true);
				if (!char.IsControl(key.KeyChar))
				{
					input += key.KeyChar;
					Console.Write("*");
				}
				else if (key.Key == ConsoleKey.Backspace && input.Length > 0)
				{
					input = input.Substring(0, input.Length - 1);
					Console.Write("\b \b");
				}
			} while (key.Key != ConsoleKey.Enter);

			Console.WriteLine();
			return input;
		}
		#endregion

		#region Get API Collection
		public static async Task GetApiCollection()
		{
			apiNo = 0;
			Console.WriteLine("\n\nAvailable endpoints :");
			foreach (var epu in endpointUrls)
			{
				apiNo++;
				Console.WriteLine(apiNo + " : " + epu);
			}

			Console.Write("\nChoose an endpoint (enter the number): ");
			endpointIndex = int.Parse(Console.ReadLine()) - 1;
			if (endpointIndex < 0 || endpointIndex >= endpointUrls.Length)
			{
				Console.WriteLine("Invalid endpoint selection.");
				return;
			}
			endpointUrl = endpointUrls[endpointIndex];
			await CallAPI();
		}
		#endregion

		#region Call API
		public static async Task CallAPI()
		{
			await GetInput();
			if (endpointUrl == "BATCanteen/GetAllActiveEmployeeInformation")
			{
				await AllActiveEmployee();
			}
			else if (endpointUrl == "BATCanteen/GetAttendanceInformation")
			{
				await AttendanceInformation();
			}
			else if (endpointUrl == "BATCanteen/GetOvertimeInformation")
			{
				await OvertimeInformation();
			}
			else if (endpointUrl == "BATCanteen/GetShiftGroupMapping")
			{
				await ShiftGroupMapping();
			}
			else if (endpointUrl == "BATCanteen/GetRelayMapping")
			{
				await RelayMapping();
			}
			else if (endpointUrl == "BATCanteen/GetUpdatedEmployeesInformation")
			{
				await UpdatedEmployeesInformation();
			}
			else if (endpointUrl == "BATCanteen/GetNewCreatedEmployeesInformation")
			{
				await CreatedEmployeesInformation();
			}
			else if (endpointUrl == "BATCanteen/GetUpdatedOvertimeInformation")
			{
				await UpdatedOvertimeInformation();
			}
			else
			{
				return;
			}
		}
		#endregion

		#region Get Input
		public static async Task GetInput()
		{
			if (endpointUrl == "BATCanteen/GetAttendanceInformation" || endpointUrl == "BATCanteen/GetOvertimeInformation" || endpointUrl == "BATCanteen/GetShiftGroupMapping" || endpointUrl == "BATCanteen/GetUpdatedEmployeesInformation" || endpointUrl == "BATCanteen/GetNewCreatedEmployeesInformation" || endpointUrl == "BATCanteen/GetUpdatedOvertimeInformation")
			{
				Console.Write("Enter FromDate (Format : dd MMM yyyy): ");
				if (!DateTime.TryParseExact(Console.ReadLine(), "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate))
				{
					Console.WriteLine("Invalid date format.");
					return;
				}

				Console.Write("Enter ToDate   (Format : dd MMM yyyy): ");
				if (!DateTime.TryParseExact(Console.ReadLine(), "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate))
				{
					Console.WriteLine("Invalid date format.");
					return;
				}

				if (toDate < fromDate)
				{
					Console.WriteLine("To Date cannot be earlier than From Date.");
					return;
				}
			}
		}
		#endregion

		#region GetAllEmployeeInformation
		public static async Task AllActiveEmployee()
		{
			using (var httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
				HttpResponseMessage getEmpResponse = await httpClient.GetAsync(baseUrl + endpointUrl);
				if (getEmpResponse.IsSuccessStatusCode)
				{
					string responseContent = await getEmpResponse.Content.ReadAsStringAsync();
					DataSet dataSet = JsonConvert.DeserializeObject<DataSet>(responseContent);
					Console.WriteLine("Active Employees:");
					DisplayDataSet(dataSet);
				}
				else
				{
					Console.WriteLine($"Error: {getEmpResponse.StatusCode}");
				}
			}
			await GetApiCollection();
		}
		#endregion

		#region GetAllAttendanceInformation
		public static async Task AttendanceInformation()
		{
			using (var httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
				var requestData = new { FromDate = fromDate, ToDate = toDate };
				jsonContent = JsonConvert.SerializeObject(requestData);
				var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
				string param = $"{fromDate.ToString("yyyy-MM-dd")}/{toDate.ToString("yyyy-MM-dd")}";
				HttpResponseMessage response = await httpClient.GetAsync(baseUrl + endpointUrl +"/" + param);
				if (response.IsSuccessStatusCode)
				{
					string responseContent = await response.Content.ReadAsStringAsync();
					DataSet dataSet = JsonConvert.DeserializeObject<DataSet>(responseContent);
					Console.WriteLine("Attendance Information:");
					DisplayDataSet(dataSet);
				}
				else
				{
					Console.WriteLine($"Error: {response.StatusCode}");
				}
			}
			await GetApiCollection();
		}

		#endregion

		#region GetAllOvertimeInformation
		public static async Task OvertimeInformation()
		{
			using (var httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
				var requestData = new { FromDate = fromDate, ToDate = toDate };
				jsonContent = JsonConvert.SerializeObject(requestData);
				var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
				string param = $"{fromDate.ToString("yyyy-MM-dd")}/{toDate.ToString("yyyy-MM-dd")}";
				HttpResponseMessage response = await httpClient.GetAsync(baseUrl + endpointUrl + "/" + param);
				if (response.IsSuccessStatusCode)
				{
					string responseContent = await response.Content.ReadAsStringAsync();
					DataSet dataSet = JsonConvert.DeserializeObject<DataSet>(responseContent);
					Console.WriteLine("Overtime Information:");
					DisplayDataSet(dataSet);
				}
				else
				{
					Console.WriteLine($"Error: {response.StatusCode}");
				}
			}
			await GetApiCollection();
		}
		#endregion

		#region GetShiftGroupMapping
		public static async Task ShiftGroupMapping()
		{
			using (var httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
				var requestData = new { FromDate = fromDate, ToDate = toDate };
				jsonContent = JsonConvert.SerializeObject(requestData);
				var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
				string param = $"{fromDate.ToString("yyyy-MM-dd")}/{toDate.ToString("yyyy-MM-dd")}";
				HttpResponseMessage response = await httpClient.GetAsync(baseUrl + endpointUrl + "/" + param);
				if (response.IsSuccessStatusCode)
				{
					string responseContent = await response.Content.ReadAsStringAsync();
					DataSet dataSet = JsonConvert.DeserializeObject<DataSet>(responseContent);
					Console.WriteLine("Shift Group Mapping:");
					DisplayDataSet(dataSet);
				}
				else
				{
					Console.WriteLine($"Error: {response.StatusCode}");
				}
			}
			await GetApiCollection();
		}
		#endregion

		#region GetRelayMapping
		public static async Task RelayMapping()
		{
			using (var httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
				HttpResponseMessage getEmpResponse = await httpClient.GetAsync(baseUrl + endpointUrl);
				if (getEmpResponse.IsSuccessStatusCode)
				{
					string responseContent = await getEmpResponse.Content.ReadAsStringAsync();
					DataSet dataSet = JsonConvert.DeserializeObject<DataSet>(responseContent);
					Console.WriteLine("Relay Mapping:");
					DisplayDataSet(dataSet);
				}
				else
				{
					Console.WriteLine($"Error: {getEmpResponse.StatusCode}");
				}
			}
			await GetApiCollection();
		}
		#endregion

		#region GetUpdatedEmployeesInformation
		public static async Task UpdatedEmployeesInformation()
		{
			using (var httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
				var requestData = new { FromDate = fromDate, ToDate = toDate };
				jsonContent = JsonConvert.SerializeObject(requestData);
				var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
				string param = $"{fromDate.ToString("yyyy-MM-dd")}/{toDate.ToString("yyyy-MM-dd")}";
				HttpResponseMessage response = await httpClient.GetAsync(baseUrl + endpointUrl + "/" + param);
				if (response.IsSuccessStatusCode)
				{
					string responseContent = await response.Content.ReadAsStringAsync();
					DataSet dataSet = JsonConvert.DeserializeObject<DataSet>(responseContent);
					Console.WriteLine("Updated Employee Information:");
					DisplayDataSet(dataSet);
				}
				else
				{
					Console.WriteLine($"Error: {response.StatusCode}");
				}
			}
			await GetApiCollection();
		}
		#endregion

		#region GetCreatedEmployeesInformation
		public static async Task CreatedEmployeesInformation()
		{
			using (var httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
				var requestData = new { FromDate = fromDate, ToDate = toDate };
				jsonContent = JsonConvert.SerializeObject(requestData);
				var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
				string param = $"{fromDate.ToString("yyyy-MM-dd")}/{toDate.ToString("yyyy-MM-dd")}";
				HttpResponseMessage response = await httpClient.GetAsync(baseUrl + endpointUrl + "/" + param);
				if (response.IsSuccessStatusCode)
				{
					string responseContent = await response.Content.ReadAsStringAsync();
					DataSet dataSet = JsonConvert.DeserializeObject<DataSet>(responseContent);
					Console.WriteLine("Created Employee Information:");
					DisplayDataSet(dataSet);
				}
				else
				{
					Console.WriteLine($"Error: {response.StatusCode}");
				}
			}
			await GetApiCollection();
		}
		#endregion

		#region GetUpdatedOvertimeInformation
		public static async Task UpdatedOvertimeInformation()
		{
			using (var httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
				var requestData = new { FromDate = fromDate, ToDate = toDate };
				jsonContent = JsonConvert.SerializeObject(requestData);
				var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
				string param = $"{fromDate.ToString("yyyy-MM-dd")}/{toDate.ToString("yyyy-MM-dd")}";
				HttpResponseMessage response = await httpClient.GetAsync(baseUrl + endpointUrl + "/" + param);
				if (response.IsSuccessStatusCode)
				{
					string responseContent = await response.Content.ReadAsStringAsync();
					DataSet dataSet = JsonConvert.DeserializeObject<DataSet>(responseContent);
					Console.WriteLine("Updated Overtime Information:");
					DisplayDataSet(dataSet);
				}
				else
				{
					Console.WriteLine($"Error: {response.StatusCode}");
				}
			}
			await GetApiCollection();
		}
		#endregion

		#region Display Dataset
		static void DisplayDataSet(DataSet dataSet)
		{
			Console.WriteLine("Active Employees:");
			DataTable table = dataSet.Tables[0];
			for (int i = 0; i < table.Columns.Count; i++)
			{
				Console.Write($"{table.Columns[i].ColumnName.PadRight(20)}");
			}
			Console.WriteLine();

			foreach (DataRow row in table.Rows)
			{
				for (int i = 0; i < table.Columns.Count; i++)
				{
					Console.Write($"{(row[i]?.ToString() ?? "").PadRight(20)}");
				}
				Console.WriteLine();
			}
		}
		#endregion
	}
}
