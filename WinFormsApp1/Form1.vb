Imports ISchemm.DurationFinder

Public Class Form1
    Private Async Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim ts = Await Providers.All.GetDurationAsync(New Uri(TextBox1.Text))
        MsgBox(ts.ToString())
    End Sub
End Class
