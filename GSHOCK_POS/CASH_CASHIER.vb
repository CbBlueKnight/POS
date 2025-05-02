Imports System.Data.SqlClient

Public Class CASH_CASHIER
    Dim con As New SqlConnection("Data Source=DESKTOP-UD7BN0F;Initial Catalog=gshock;Integrated Security=True")

    Private Sub btnConfirm_Click(sender As Object, e As EventArgs) Handles btnConfirm.Click
        Dim total As Decimal
        Dim payment As Decimal
        Dim change As Decimal

        If Not Decimal.TryParse(txtTotal.Text, total) Then
            MessageBox.Show("Invalid total amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        If Not Decimal.TryParse(txtPayment.Text, payment) Then
            MessageBox.Show("Please enter a valid payment amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        If payment < total Then
            MessageBox.Show("Payment must be equal to or greater than the total.", "Insufficient Payment", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        change = payment - total
        txtChange.Text = change.ToString("F2")


        Try
            con.Open()
            Dim cmd As New SqlCommand("UPDATE sum SET payment = @payment, change = @change", con)
            cmd.Parameters.AddWithValue("@payment", payment)
            cmd.Parameters.AddWithValue("@change", change)
            cmd.ExecuteNonQuery()

            MessageBox.Show("Payment confirmed and saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Error saving data: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            con.Close()
        End Try
    End Sub

    Private Sub LoadTotal()
        Try
            con.Open()
            Dim cmd As New SqlCommand("SELECT TOP 1 total FROM sum ORDER BY total DESC", con)
            Dim reader As SqlDataReader = cmd.ExecuteReader()
            If reader.Read() Then
                txtTotal.Text = reader("total").ToString()
            End If
            reader.Close()
        Catch ex As Exception
            MessageBox.Show("Error loading total: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            con.Close()
        End Try
    End Sub

    Private Sub txtPayment_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtPayment.KeyPress
        Dim txt As TextBox = CType(sender, TextBox)

        If Char.IsControl(e.KeyChar) Then
            Return
        End If

        If Char.IsDigit(e.KeyChar) Then
            Return
        End If

        If e.KeyChar = "."c Then
            If txt.Text.Contains(".") OrElse txt.SelectionStart = 0 Then
                e.Handled = True
            End If
            Return
        End If

        e.Handled = True
    End Sub


    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        PAYMENT.Show()
        Me.Hide()
    End Sub

    Private Sub MP_Click(sender As Object, e As EventArgs) Handles MP.Click
        PRODUCT_LOOK_UP.Show()
        Me.Hide()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        PAYMENT.Show()
        Me.Hide()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Application.Exit()
    End Sub

    Private Sub CASH_CASHIER_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadTotal()
    End Sub
End Class
