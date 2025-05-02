Imports System.Data.SqlClient

Public Class ADMIN_LOGIN
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            con.Open()
            Dim query As String = "SELECT COUNT(*) FROM login WHERE username = @user AND password = @pass AND position = 'admin'"
            cmd = New SqlCommand(query, con)
            cmd.Parameters.AddWithValue("@user", TextBox1.Text)
            cmd.Parameters.AddWithValue("@pass", TextBox2.Text)

            Dim result As Integer = CInt(cmd.ExecuteScalar())

            If result = 1 Then
                MsgBox("Login successful!", vbInformation)
                ADMIN_SALES_OVERVIEW.Show()
                Me.Hide()
            Else
                MsgBox("Login failed or not authorized!", vbCritical)
            End If
        Catch ex As Exception
            MsgBox("Error: " & ex.Message, vbCritical)
        Finally
            con.Close()
        End Try
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub

    Private Sub ADMIN_LOGIN_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        opencon()
        con.Close()
    End Sub
End Class