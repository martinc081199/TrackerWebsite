using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseHttpsRedirection();

// ----------------- HOME PAGE: FORM -----------------
app.MapGet("/", () =>
{
    var html = @"<!DOCTYPE html>
<html>
<head>
    <title>Weekly Step Tracker</title>
</head>
<body style='font-family: Arial, sans-serif; background-color:#f5f5f5;'>
    <div style='max-width: 480px; margin: 40px auto; padding: 20px; background:white; border-radius:8px; box-shadow:0 0 8px rgba(0,0,0,0.1);'>
        <h1 style='text-align:center;'>Weekly Step Tracker</h1>

        <form method='post' action='/result' style='margin-top:20px;'>
            <label>Your name:</label><br/>
            <input type='text' name='name' style='width:100%; padding:6px; margin-bottom:10px;' /><br/>

            <label>Email:</label><br/>
            <input type='email' name='email' style='width:100%; padding:6px; margin-bottom:10px;' /><br/>

            <label>User ID:</label><br/>
            <input type='text' name='userid' style='width:100%; padding:6px; margin-bottom:15px;' /><br/>

            <label>Monday steps:</label><br/>
            <input type='text' name='mon' style='width:100%; padding:6px; margin-bottom:8px;' /><br/>

            <label>Tuesday steps:</label><br/>
            <input type='text' name='tue' style='width:100%; padding:6px; margin-bottom:8px;' /><br/>

            <label>Wednesday steps:</label><br/>
            <input type='text' name='wed' style='width:100%; padding:6px; margin-bottom:8px;' /><br/>

            <label>Thursday steps:</label><br/>
            <input type='text' name='thu' style='width:100%; padding:6px; margin-bottom:8px;' /><br/>

            <label>Friday steps:</label><br/>
            <input type='text' name='fri' style='width:100%; padding:6px; margin-bottom:8px;' /><br/>

            <label>Saturday steps:</label><br/>
            <input type='text' name='sat' style='width:100%; padding:6px; margin-bottom:8px;' /><br/>

            <label>Sunday steps:</label><br/>
            <input type='text' name='sun' style='width:100%; padding:6px; margin-bottom:15px;' /><br/>

            <label>
                <input type='checkbox' name='sendemail' value='yes' />
                Email me a summary of my week (demo only)
            </label><br/><br/>

            <button type='submit' style='width:100%; padding:10px; background:#0078d4; color:white; border:none; border-radius:4px; cursor:pointer;'>
                Calculate weekly average
            </button>
        </form>
    </div>
</body>
</html>";

    return Results.Content(html, "text/html");
});

// ----------------- RESULT PAGE -----------------
app.MapPost("/result", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();

    // Build user object
    var tracker = new User();
    tracker.Name = form["name"].ToString();
    tracker.Email = form["email"].ToString();

    var userIdString = form["userid"].ToString();
    if (!int.TryParse(userIdString, out tracker.UserId))
    {
        tracker.UserId = 0; // default if invalid
    }

    if (string.IsNullOrWhiteSpace(tracker.Name))
    {
        tracker.Name = "Unknown";
    }

    // Read weekly steps
    int[] weeklyArray = new int[7];

    int ParseSteps(string key)
    {
        var value = form[key].ToString();
        return int.TryParse(value, out int steps) ? steps : 0;
    }

    weeklyArray[0] = ParseSteps("mon");
    weeklyArray[1] = ParseSteps("tue");
    weeklyArray[2] = ParseSteps("wed");
    weeklyArray[3] = ParseSteps("thu");
    weeklyArray[4] = ParseSteps("fri");
    weeklyArray[5] = ParseSteps("sat");
    weeklyArray[6] = ParseSteps("sun");

    int total = 0;
    foreach (int step in weeklyArray)
    {
        total += step;
    }

    tracker.AvgSteps = total / 7;

    string recommendation = tracker.GetRecommendationText();

    bool sendEmailRequested = form["sendemail"] == "yes";

    var html = $@"<!DOCTYPE html>
<html>
<head>
    <title>Weekly Step Result</title>
</head>
<body style='font-family: Arial, sans-serif; background-color:#f5f5f5;'>
    <div style='max-width: 480px; margin: 40px auto; padding: 20px; background:white; border-radius:8px; box-shadow:0 0 8px rgba(0,0,0,0.1);'>
        <h1>Results for {tracker.Name}</h1>

        <p><strong>User ID:</strong> {tracker.UserId}</p>
        <p><strong>Email:</strong> {tracker.Email}</p>

        <p><strong>Total weekly steps:</strong> {total}</p>
        <p><strong>Average daily steps:</strong> {tracker.AvgSteps}</p>
        <p><strong>Recommendation:</strong> {recommendation}</p>";

    if (sendEmailRequested && !string.IsNullOrWhiteSpace(tracker.Email))
    {
        html += $@"
        <p style='margin-top:15px; color:green;'>
            Demo: a summary would be sent to <strong>{tracker.Email}</strong>.
        </p>";
    }

    html += @"
        <p style='margin-top:20px;'>
            <a href='/' style='text-decoration:none; color:#0078d4;'>Go back</a>
        </p>
    </div>
</body>
</html>";

    return Results.Content(html, "text/html");
});

app.Run();

// ----------------- USER CLASS -----------------
class User
{
    public string Name = "";
    public string Email = "";
    public int UserId;
    public int AvgSteps;

    public string GetRecommendationText()
    {
        if (AvgSteps < 10000)
        {
            return "Aim for around 10,000 steps per day with our supportive features.";
        }
        else
        {
            return "You are very active. Consider advanced tracking features.";
        }
    }
}