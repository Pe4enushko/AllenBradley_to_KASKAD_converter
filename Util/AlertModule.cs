using ABKaskadGUI;

public static class AlertModule
{
    public static async Task Alert(string message)
    {
        await App.Current.MainPage.DisplayAlert("Info", message, "Ok");
    }
}