using CommunityToolkit.Maui.Storage;

namespace ABKaskadGUI.ViewModels
{
public class MainViewModel : BaseViewModel
{
    bool _ignoreHidden = true;
    public bool IgnoreHidden 
    {
        get => _ignoreHidden;
        set
        {
            _ignoreHidden = value;
            OnPropertyChanged();
        }
    }

    public Command ConvertCommand { get; set; }
    public MainViewModel()
    {
        ConvertCommand = new(Convert);
    }

    public async void Convert()
    {
        List<string> input = new();
		var files = await FilePicker.PickMultipleAsync();
		foreach (var file in files)
		{
			input.Add(file.FullPath);
		}
		try
		{
			var content = new WorkingPart().Convert(input, IgnoreHidden);

			await AlertModule.Alert("После закрытия диалога выберите директорию для сохранения файла. Там появится файл");
			var picked = await FolderPicker.PickAsync(CancellationToken.None);

			if (picked.IsSuccessful)
				File.WriteAllText(picked.Folder.Path + "\\KaskadImport.dpl", content);
			else
				await AlertModule.Alert("Директория не выбрана");
		}
		catch(Exception exc)
		{
			await AlertModule.Alert(exc.Message);
		}
    }    
}
}