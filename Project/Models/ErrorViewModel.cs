namespace Project.Models
{
	public class ErrorViewModel
	{
		// Variable set by the Error action method in the HomeController
		public string? RequestId { get; set; }

		/* If the RequestId handled in the Error action method inside the HomeController is null
		   then assign the boolean False to ShowRequestId, else boolean will be True.*/
		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

	}
}
