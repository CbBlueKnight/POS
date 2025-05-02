Public Class MANAGER_SALES_OVERVIEW
    Private Sub C1_Click(sender As Object, e As EventArgs) Handles C1.Click
        ADMIN_C1.Show()
        Me.Hide()
    End Sub

    Private Sub C2_Click(sender As Object, e As EventArgs) Handles C2.Click
        ADMIN_C2.Show()
        Me.Hide()
    End Sub

    Private Sub C3_Click(sender As Object, e As EventArgs) Handles C3.Click

    End Sub

    Private Sub DAILY_Click(sender As Object, e As EventArgs) Handles DAILY.Click
        ADMIN_DAILY.Show()
        Me.Hide()
    End Sub

    Private Sub WEEKLY_Click(sender As Object, e As EventArgs) Handles WEEKLY.Click
        ADMIN_WEEKLY.Show()
        Me.Hide()
    End Sub

    Private Sub MONTHLY_Click(sender As Object, e As EventArgs) Handles MONTHLY.Click
        ADMIN_MONTHLY.Show()
        Me.Hide()
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Application.Exit()
    End Sub

    Private Sub MANAGER_SALES_OVERVIEW_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub MP_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        ADD_USERS.Show()
        Me.Hide()
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        STARTUP.Show()
        Me.Hide()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        PRODUCT_WEB.Show()
        Me.Hide()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        PROMO_WEB.Show()
        Me.Hide()
    End Sub
End Class