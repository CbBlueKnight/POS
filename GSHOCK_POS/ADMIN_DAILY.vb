Imports Microsoft.Data.SqlClient
Imports Dapper
Imports System.Windows.Forms.DataVisualization.Charting

Public Class ADMIN_DAILY
    Private Sub btnLoadSales_Click(sender As Object, e As EventArgs) Handles btnLoadSales.Click
        LoadHourlyData()
    End Sub

    Public Function GetSalesByHour(connectionString As String, selectedDay As Date) As List(Of HourlySaleRecord)
        Try
            Using conn As New SqlConnection(connectionString)
                conn.Open()
                Dim sql = "
                SELECT 
                    DATEPART(HOUR, SaleDate) AS SaleHour,
                    SUM(Amount) AS TotalAmount
                FROM SalesReport
                WHERE CAST(SaleDate AS DATE) = @SelectedDay
                GROUP BY DATEPART(HOUR, SaleDate)
                ORDER BY SaleHour"
                Return conn.Query(Of HourlySaleRecord)(sql, New With {.SelectedDay = selectedDay}).ToList()
            End Using
        Catch ex As Exception
            MessageBox.Show("An error occurred while retrieving hourly sales: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return New List(Of HourlySaleRecord)()
        End Try
    End Function

    Private Sub LoadHourlyData()
        Try
            Dim selectedDate As Date = DateTimePicker1.Value.Date
            Dim reportData = GetSalesByHour(My.Settings.ConStr, selectedDate)

            If Chart1.ChartAreas.Count = 0 Then
                Chart1.ChartAreas.Add("MainArea")
            End If

            If Not Chart1.Series.IsUniqueName("Hourly Sales") Then
                Chart1.Series.Remove(Chart1.Series("Hourly Sales"))
            End If

            Dim series = Chart1.Series.Add("Hourly Sales")
            With series
                .ChartType = DataVisualization.Charting.SeriesChartType.Column
                .Points.Clear()

                For hour As Integer = 0 To 23
                    Dim sale = reportData.FirstOrDefault(Function(r) r.SaleHour = hour)
                    Dim amount = If(sale IsNot Nothing, sale.TotalAmount, 0D)
                    .Points.AddXY($"{hour}:00", amount)
                Next
            End With

            Label3.Text = "Sales on " & selectedDate.ToString("dddd, dd MMMM yyyy")

        Catch ex As Exception
            MessageBox.Show("An error occurred while loading hourly data: " & ex.Message, "Chart Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


End Class