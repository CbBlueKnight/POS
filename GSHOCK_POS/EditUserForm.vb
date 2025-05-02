Imports System.Data.SqlClient
Imports System.IO

Public Class EditUserForm
    Private username As String
    Dim connectionString As String = "Data Source=DESKTOP-UD7BN0F;Initial Catalog=gshock;Integrated Security=True"

    Public Sub New(user As String)
        InitializeComponent()
        username = user
    End Sub

    Private Sub EditUserForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Populate dropdown for positions
        cmbPosition.Items.Clear()
        cmbPosition.Items.AddRange(New String() {"Cashier", "Manager", "Admin"})

        Using conn As New SqlConnection(connectionString)
            Dim cmd As New SqlCommand("SELECT * FROM login WHERE Username=@Username", conn)
            cmd.Parameters.AddWithValue("@Username", username)
            conn.Open()
            Using reader = cmd.ExecuteReader()
                If reader.Read() Then
                    txtUsername.Text = reader("Username").ToString()
                    txtPassword.Text = reader("Password").ToString()
                    txtName.Text = reader("Name").ToString()
                    cmbPosition.SelectedItem = reader("Position").ToString()
                End If
            End Using
        End Using
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If cmbPosition.SelectedItem Is Nothing Then
            MessageBox.Show("Please select a position.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Using conn As New SqlConnection(connectionString)
            Dim cmd As New SqlCommand("UPDATE login SET Username=@NewUsername, Password=@Password, Name=@Name, Position=@Position WHERE Username=@OldUsername", conn)
            cmd.Parameters.AddWithValue("@NewUsername", txtUsername.Text.Trim())
            cmd.Parameters.AddWithValue("@Password", txtPassword.Text.Trim())
            cmd.Parameters.AddWithValue("@Name", txtName.Text.Trim())
            cmd.Parameters.AddWithValue("@Position", cmbPosition.SelectedItem.ToString())
            cmd.Parameters.AddWithValue("@OldUsername", username)
            conn.Open()
            cmd.ExecuteNonQuery()
        End Using

        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    Private Sub txtPassword_TextChanged(sender As Object, e As EventArgs)

    End Sub
End Class
