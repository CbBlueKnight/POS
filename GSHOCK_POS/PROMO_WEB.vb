Imports System.Data.SqlClient

Public Class PROMO_WEB
    Dim connectionString As String = "Data Source=DESKTOP-UD7BN0F;Initial Catalog=gshock;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Dim id As String = txtId.Text.Trim()
        Dim name As String = txtName.Text.Trim()
        Dim discountText As String = txtDiscount.Text.Trim()
        Dim expiryDate As Date = dtpExpiryDate.Value

        Dim discount As Decimal
        If Not Decimal.TryParse(discountText, discount) OrElse discount < 0 OrElse discount > 100 Then
            MessageBox.Show("Please enter a valid discount between 0 and 100.", "Invalid Discount", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim connStr As String = connectionString
        Using conn As New SqlConnection(connStr)
            Dim cmd As New SqlCommand("INSERT INTO Voucher (Id, Name, Discount, ExpiryDate) VALUES (@Id, @Name, @Discount, @ExpiryDate)", conn)
            cmd.Parameters.AddWithValue("@Id", id)
            cmd.Parameters.AddWithValue("@Name", name)
            cmd.Parameters.AddWithValue("@Discount", discount)
            cmd.Parameters.AddWithValue("@ExpiryDate", expiryDate)

            conn.Open()
            cmd.ExecuteNonQuery()
            MessageBox.Show("Voucher saved successfully.")
        End Using
    End Sub

    Private Sub btnRemoveExpired_Click(sender As Object, e As EventArgs) Handles btnRemoveExpired.Click
        Dim connStr As String = connectionString
        Dim removedCount As Integer = 0

        Try
            Using conn As New SqlConnection(connStr)
                Dim cmd As New SqlCommand("DELETE FROM Voucher WHERE CAST(ExpiryDate AS DATE) <= @Today", conn)
                cmd.Parameters.AddWithValue("@Today", DateTime.Today)

                conn.Open()
                removedCount = cmd.ExecuteNonQuery()
            End Using

            MessageBox.Show($"{removedCount} expired voucher(s) removed (based on date only).", "Cleanup Successful", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("An error occurred while removing expired vouchers: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles Button1.Click
        MANAGER_SALES_OVERVIEW.Show()
        Me.Hide()
    End Sub
End Class
