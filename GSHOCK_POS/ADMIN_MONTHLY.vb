Imports Microsoft.Data.SqlClient
Imports Dapper

Public Class ADMIN_MONTHLY

    Public Function GetSalesForMonth(connectionString As String, monthStart As Date, monthEnd As Date) As List(Of SaleRecord)
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

                Return conn.Query(Of SaleRecord)(sql, New With {.StartDate = monthStart, .EndDate = monthEnd}).ToList()
            End Using
        Catch ex As Exception
            MessageBox.Show("An error occurred while retrieving data: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return New List(Of SaleRecord)()
        End Try
    End Function


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        LoadData()
    End Sub

    Private Sub LoadData()
        Try
            Dim selectedDate As Date = DateTimePicker1.Value.Date
            Dim monthStart As New Date(selectedDate.Year, selectedDate.Month, 1)
            Dim monthEnd As Date = monthStart.AddMonths(1).AddDays(-1)

            Dim reportData = GetSalesForMonth(My.Settings.ConStr, monthStart, monthEnd)

            If Chart1.ChartAreas.Count = 0 Then
                Chart1.ChartAreas.Add("MainArea")
            End If

            If Not Chart1.Series.IsUniqueName("Monthly Sales") Then
                Chart1.Series.Remove(Chart1.Series("Monthly Sales"))
            End If

            Dim series = Chart1.Series.Add("Monthly Sales")
            With series
                .ChartType = DataVisualization.Charting.SeriesChartType.Column
                .Points.Clear()

                For i As Integer = 0 To (monthEnd - monthStart).Days
                    Dim day As Date = monthStart.AddDays(i)
                    Dim sale = reportData.FirstOrDefault(Function(r) r.SaleDate.Date = day)
                    Dim amount = If(sale IsNot Nothing, sale.TotalAmount, 0D)
                    .Points.AddXY(day.ToString("dd"), amount)
                Next
            End With

            Label3.Text = "Month of " & monthStart.ToString("MMMM yyyy")

        Catch ex As Exception
            MessageBox.Show("An error occurred while loading the chart: " & ex.Message, "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


End Class
