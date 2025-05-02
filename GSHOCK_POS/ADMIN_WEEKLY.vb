Imports Microsoft.Data.SqlClient
Imports Dapper
Imports System.Globalization

Public Class ADMIN_WEEKLY
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        LoadWeeklyData()
    End Sub

    Public Function GetSalesByDayOfWeek(connectionString As String, weekStart As Date, weekEnd As Date) As List(Of DailySaleRecord)
        Try
            Using conn As New SqlConnection(connectionString)
                conn.Open()
                Dim sql = "
                SELECT 
                    CAST(SaleDate AS DATE) AS SaleDate,
                    SUM(Amount) AS TotalAmount
                FROM SalesReport
                WHERE SaleDate BETWEEN @StartDate AND @EndDate
                GROUP BY CAST(SaleDate AS DATE)
                ORDER BY SaleDate"
                Return conn.Query(Of DailySaleRecord)(sql, New With {.StartDate = weekStart, .EndDate = weekEnd}).ToList()
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading weekly sales: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return New List(Of DailySaleRecord)()
        End Try
    End Function

    Private Sub LoadWeeklyData()
        Try
            Dim selectedDate As Date = DateTimePicker1.Value.Date

            Dim weekStart As Date = selectedDate.AddDays(-(CInt(selectedDate.DayOfWeek)))
            Dim weekEnd As Date = weekStart.AddDays(6)

            Dim reportData = GetSalesByDayOfWeek(My.Settings.ConStr, weekStart, weekEnd)

            If Chart1.ChartAreas.Count = 0 Then
                Chart1.ChartAreas.Add("MainArea")
            End If

            If Not Chart1.Series.IsUniqueName("Weekly Sales") Then
                Chart1.Series.Remove(Chart1.Series("Weekly Sales"))
            End If

            Dim series = Chart1.Series.Add("Weekly Sales")
            With series
                .ChartType = DataVisualization.Charting.SeriesChartType.Column
                .Points.Clear()

                For i As Integer = 0 To 6
                    Dim day = weekStart.AddDays(i)
                    Dim sale = reportData.FirstOrDefault(Function(r) r.SaleDate.Date = day)
                    Dim amount = If(sale IsNot Nothing, sale.TotalAmount, 0D)
                    .Points.AddXY(day.ToString("ddd"), amount)
                Next
            End With

            Label3.Text = $"Week: {weekStart:dd MMM} - {weekEnd:dd MMM yyyy}"

        Catch ex As Exception
            MessageBox.Show("Error loading weekly chart: " & ex.Message, "Chart Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
