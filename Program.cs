using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/", () =>
{
    var html = @"<!DOCTYPE html>
<html>
<head>
    <title>Weekly Step Tracker</title>
</head>
<body style='font-family: Arial, sans-serif; background:#f5f5f5;'>
    <div style='max-width:480px; margin:40px auto; padding:20px; background:white; border-radius:6px; box-shadow:0 0 6px rgba(0,0,0,0.15);'>
        <h1 style='text-align:center; margin-bottom:10px;'>Weekly Step Tracker</h1>
        <p style='font-size:14px; color:#555;'>
            Enter your steps for each day and see your weekly average.
        </p>

        <form method='post' action='/result' style='margin-top:15px;'>
            <label>Your name:</label><br/>
            <input type='text' name='name' style='width:100%; padding:6px; margin:4px 0 10px 0;' /><br/>

            <label>Email:</label><br/>
            <input type='email' name='email' style='width:100%; padding:6px; margin:4px 0 10px 0;' /><br/>

            <label>User ID:</label><br/>
            <input type='text' name='userid' style='width:100%; padding:6px; margin:4px 0 12px 0;' /><br/>

            <label>Monday steps:</label><br/>
            <input type='text' name='mon' style='width:100%; padding:6px; margin-bottom:6px;' /><br/>

            <label>Tuesday steps:</label><br/>
            <input type='text' name='tue' style='width:100%; padding:6px; margin-bottom:6px;' /><br/>

            <label>Wednesday steps:</label><br/>
            <input type='text' name='wed' style='width:100%; padding:6px; margin-bottom:6px;' /><br/>

            <label>Thursday steps:</label><br/>
            <input type='text' name='thu' style='width:100%; padding:6px; margin-bottom:6px;' /><br/>

            <label>Friday steps:</label><br/>
            <input type='text' name='fri' style='width:100%; padding:6px; margin-bottom:6px;' /><br/>

            <label>Saturday steps:</label><br/>
            <input type='text' name='sat' style='width:100%; padding:6px; margin-bottom:6px;' /><br/>

            <label>Sunday steps:</label><br/>
            <input type='text' name='sun' style='width:100%; padding:6px; margin-bottom:12px;' /><br/>

            <label style='font-size:13px;'>
                <input type='checkbox' name='sendemail' value='yes' />
                Email me a summary (demo only)
            </label><br/><br/>

            <button type='submit' 
                    style='width:100%; padding:8px; background:#0078d4; color:white; border:none; border-radius:4px; cursor:pointer;'>
                Calculate weekly average
            </button>
        </form>
    </div>
</body>
</html>";

    return Results.Content(html, "text/html");
});


app.MapPost("/result", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();

    var user = new User();
    user.Name  = form["name"].ToString();
    user.Email = form["email"].ToString();

    var userIdText = form["userid"].ToString();
    if (!int.TryParse(userIdText, out user.UserId))
    {
        user.UserId = 0; 
    }

    if (string.IsNullOrWhiteSpace(user.Name))
    {
        user.Name = "Unknown";
    }

    // Read steps for each day
    int[] weeklySteps = new int[7];

    int GetSteps(string key)
    {
        var value = form[key].ToString();
        return int.TryParse(value, out int steps) ? steps : 0;
    }

    weeklySteps[0] = GetSteps("mon");
    weeklySteps[1] = GetSteps("tue");
    weeklySteps[2] = GetSteps("wed");
    weeklySteps[3] = GetSteps("thu");
    weeklySteps[4] = GetSteps("fri");
    weeklySteps[5] = GetSteps("sat");
    weeklySteps[6] = GetSteps("sun");

    int total = 0;
    foreach (var s in weeklySteps)
    {
        total += s;
    }

    user.AvgSteps = total / 7;
    string recommendation = user.GetRecommendationText();

    bool sendEmail = form["sendemail"] == "yes";

    var resultHtml = $@"<!DOCTYPE html>
<html>
<head>
    <title>Weekly Step Result</title>
</head>
<body style='font-family: Arial, sans-serif; background:#f5f5f5;'>
    <div style='max-width:480px; margin:40px auto; padding:20px; background:white; border-radius:6px; box-shadow:0 0 6px rgba(0,0,0,0.15);'>
        <h1>Results for {user.Name}</h1>

        <p><strong>User ID:</strong> {user.UserId}</p>
        <p><strong>Email:</strong> {user.Email}</p>

        <p><strong>Total weekly steps:</strong> {total}</p>
        <p><strong>Average per day:</strong> {user.AvgSteps}</p>
        <p><strong>Recommendation:</strong> {recommendation}</p>";

    if (sendEmail && !string.IsNullOrWhiteSpace(user.Email))
    {
        resultHtml += $@"
        <p style='margin-top:12px; color:green; font-size:13px;'>
            (Demo) A summary would be emailed to: <strong>{user.Email}</strong>
        </p>";
    }

    resultHtml += @"
        <p style='margin-top:18px;'>
            <a href='/' style='color:#0078d4; text-decoration:none;'>Go back</a>
        </p>
    </div>
</body>
</html>";

    return Results.Content(resultHtml, "text/html");
});

app.Run();

// Simple user model used by the app
class User
{
    public string Name  = "";
    public string Email = "";
    public int    UserId;
    public int    AvgSteps;

    public string GetRecommendationText()
    {
        if (AvgSteps < 10000)
        {
            return "Try to work towards around 10,000 steps per day.";
        }
        else
        {
            return "You're already very active, consider more advanced tracking goals.";
        }
    }
}