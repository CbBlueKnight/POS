Imports System.Data.SqlClient

Public Class PRODUCT_LOOK_UP
    Dim con As New SqlConnection("Data Source=DESKTOP-UD7BN0F;Initial Catalog=gshock;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")
    Dim dtProducts As New DataTable()
    Dim dtOrders As New DataTable()
    Private discountWarningShown As Boolean = False

    Private Sub PRODUCT_LOOK_UP_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadProducts()
        SetupOrdersTable()
        SetupDiscountControls()
    End Sub

    Private Sub LoadProducts()
        dtProducts.Clear()
        Using da As New SqlDataAdapter("SELECT * FROM products", con)
            da.Fill(dtProducts)
        End Using

        dgvProducts.DataSource = dtProducts
        dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        For Each colName In {"image", "date", "total"}
            If dgvProducts.Columns.Contains(colName) Then
                dgvProducts.Columns(colName).Visible = False
            End If
        Next
    End Sub

    Private Sub SetupOrdersTable()
        dtOrders.Columns.Clear()
        dtOrders.Columns.AddRange({
            New DataColumn("id"),
            New DataColumn("productname"),
            New DataColumn("series"),
            New DataColumn("price", GetType(Decimal)),
            New DataColumn("quantity", GetType(Integer)),
            New DataColumn("total", GetType(Decimal)),
            New DataColumn("date_added", GetType(Date))
        })

        dgvOrders.DataSource = dtOrders
        dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    End Sub

    Private Sub SetupDiscountControls()
        lblDiscountID.Visible = False
        txtDiscountID.Visible = False
        cmbDiscountType.Items.AddRange(New String() {"None", "Senior", "PWD"})
        cmbDiscountType.SelectedIndex = 0
    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        Dim dv As New DataView(dtProducts)
        dv.RowFilter = $"id LIKE '%{txtSearch.Text}%' OR productname LIKE '%{txtSearch.Text}%'"
        dgvProducts.DataSource = dv
    End Sub

    Private Sub dgvProducts_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvProducts.CellClick
        If e.RowIndex >= 0 Then
            Dim row = dgvProducts.Rows(e.RowIndex)
            txtReference.Text = row.Cells("id").Value.ToString()
            txtProductName.Text = row.Cells("productname").Value.ToString()
            txtSeries.Text = row.Cells("series").Value.ToString()
            txtPrice.Text = row.Cells("price").Value.ToString()
            txtQuantity.Text = "1"
            CalculateItemTotal()
        End If
    End Sub

    Private Sub dgvOrders_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvOrders.CellClick
        If e.RowIndex >= 0 AndAlso MessageBox.Show("Remove this item from cart?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Dim row = dgvOrders.Rows(e.RowIndex)
            Dim productId = row.Cells("id").Value.ToString()
            Dim removedQty = Convert.ToInt32(row.Cells("quantity").Value)

            dtOrders.Rows.RemoveAt(e.RowIndex)

            For Each prodRow As DataGridViewRow In dgvProducts.Rows
                If prodRow.Cells("id").Value.ToString() = productId Then
                    prodRow.Cells("quantity").Value = Convert.ToInt32(prodRow.Cells("quantity").Value) + removedQty
                    Exit For
                End If
            Next

            CalculateCartTotal()
        End If
    End Sub

    Private Sub txtQuantity_TextChanged(sender As Object, e As EventArgs) Handles txtQuantity.TextChanged, txtPrice.TextChanged
        CalculateItemTotal()
    End Sub

    Private Sub CalculateItemTotal()
        Dim price As Decimal = 0
        Dim quantity As Integer = 0
        Decimal.TryParse(txtPrice.Text, price)
        Integer.TryParse(txtQuantity.Text, quantity)
        txtTotal.Text = (price * quantity).ToString("F2")
    End Sub

    Private Sub CalculateCartTotal(Optional validateDiscount As Boolean = True)
        Dim grandTotal As Decimal = 0
        Dim highestPriceRow As DataRow = Nothing
        Dim applyDiscount = (cmbDiscountType.Text = "Senior" Or cmbDiscountType.Text = "PWD")

        For Each row As DataRow In dtOrders.Rows
            If highestPriceRow Is Nothing OrElse Convert.ToDecimal(row("price")) > Convert.ToDecimal(highestPriceRow("price")) Then
                highestPriceRow = row
            End If
        Next

        If applyDiscount Then
            If validateDiscount AndAlso Not System.Text.RegularExpressions.Regex.IsMatch(txtDiscountID.Text, "^\d{6}$") Then
                If Not discountWarningShown Then
                    MessageBox.Show($"Please enter a valid 6-digit ID for {cmbDiscountType.Text} discount.", "Invalid ID", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    discountWarningShown = True
                End If
                txtTotal.Text = "0.00"
                Return
            Else
                discountWarningShown = False
            End If
        End If

        For Each row As DataRow In dtOrders.Rows
            Dim rowTotal As Decimal = Convert.ToDecimal(row("total"))
            If row Is highestPriceRow AndAlso applyDiscount Then
                rowTotal *= 0.8D
            End If
            grandTotal += rowTotal
        Next

        txtTotal.Text = grandTotal.ToString("F2")
    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        If txtReference.Text = "" OrElse Not Integer.TryParse(txtQuantity.Text, 0) Then
            MessageBox.Show("Please select a product and enter a valid quantity.")
            Return
        End If

        Dim productId = txtReference.Text
        Dim inputQuantity = Integer.Parse(txtQuantity.Text)

        For Each row As DataGridViewRow In dgvProducts.Rows
            If row.Cells("id").Value.ToString() = productId Then
                Dim availableQty = Convert.ToInt32(row.Cells("quantity").Value)
                If inputQuantity > availableQty Then
                    MessageBox.Show("Not enough stock available.")
                    Return
                End If
                row.Cells("quantity").Value = availableQty - inputQuantity
                Exit For
            End If
        Next

        Dim existingRow = dtOrders.Rows.Cast(Of DataRow)().FirstOrDefault(Function(r) r("id").ToString() = productId)

        If existingRow IsNot Nothing Then
            Dim newQty = Convert.ToInt32(existingRow("quantity")) + inputQuantity
            existingRow("quantity") = newQty
            existingRow("total") = newQty * Convert.ToDecimal(txtPrice.Text)
        Else
            dtOrders.Rows.Add(productId, txtProductName.Text, txtSeries.Text, Convert.ToDecimal(txtPrice.Text), inputQuantity, Convert.ToDecimal(txtPrice.Text) * inputQuantity, Date.Today)
        End If

        CalculateCartTotal()
    End Sub

    Private Sub btnNewTransaction_Click(sender As Object, e As EventArgs) Handles btnNewTransaction.Click
        NewTransaction()
    End Sub

    Private Sub NewTransaction()
        ' Return quantities to stock
        For Each row As DataRow In dtOrders.Rows
            Dim productId = row("id").ToString()
            Dim qtyToReturn = Convert.ToInt32(row("quantity"))

            For Each prodRow As DataGridViewRow In dgvProducts.Rows
                If prodRow.Cells("id").Value.ToString() = productId Then
                    prodRow.Cells("quantity").Value = Convert.ToInt32(prodRow.Cells("quantity").Value) + qtyToReturn
                    Exit For
                End If
            Next
        Next

        ' Clear the in-memory cart and form fields
        dtOrders.Clear()
        txtReference.Clear()
        txtProductName.Clear()
        txtSeries.Clear()
        txtPrice.Clear()
        txtQuantity.Clear()
        txtTotal.Clear()

        ' Clear SUM and LOOKUP tables in the database
        Try
            con.Open()

            Using cmd As New SqlCommand("DELETE FROM sum; DELETE FROM lookup;", con)
                cmd.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            MessageBox.Show("Failed to clear transaction data: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            con.Close()
        End Try

        CalculateCartTotal()
    End Sub

    Private Sub cmbDiscountType_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbDiscountType.SelectedIndexChanged
        Dim isDiscount = (cmbDiscountType.Text = "Senior" Or cmbDiscountType.Text = "PWD")
        lblDiscountID.Visible = isDiscount
        txtDiscountID.Visible = isDiscount
        If Not isDiscount Then txtDiscountID.Clear()
        CalculateCartTotal()
    End Sub

    Private Sub txtDiscountID_TextChanged(sender As Object, e As EventArgs) Handles txtDiscountID.TextChanged
        CalculateCartTotal()
    End Sub

    Private Sub SaveTransaction()
        If dtOrders.Rows.Count = 0 Then
            MessageBox.Show("Cart is empty. Add products before proceeding.", "Empty Cart", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim totalAmount As Integer
        Integer.TryParse(Math.Floor(Convert.ToDecimal(txtTotal.Text)), totalAmount)

        Try
            con.Open()

            ' Save to SUM table
            Dim cmdSum As New SqlCommand("INSERT INTO sum (total, payment) VALUES (@total, @payment)", con)
            cmdSum.Parameters.AddWithValue("@total", totalAmount)
            cmdSum.Parameters.AddWithValue("@payment", 0)
            cmdSum.ExecuteNonQuery()

            ' Save order items
            For Each row As DataRow In dtOrders.Rows
                Dim cmdLookup As New SqlCommand("INSERT INTO lookup (id, productname, series, price, quantity, date, total) VALUES (@id, @productname, @series, @price, @quantity, @date, @total)", con)
                cmdLookup.Parameters.AddWithValue("@id", row("id").ToString())
                cmdLookup.Parameters.AddWithValue("@productname", row("productname").ToString())
                cmdLookup.Parameters.AddWithValue("@series", row("series").ToString())
                cmdLookup.Parameters.AddWithValue("@price", Convert.ToDecimal(row("price")))
                cmdLookup.Parameters.AddWithValue("@quantity", Convert.ToInt32(row("quantity")))
                cmdLookup.Parameters.AddWithValue("@date", Date.Today)
                cmdLookup.Parameters.AddWithValue("@total", Convert.ToDecimal(row("total")))
                cmdLookup.ExecuteNonQuery()
            Next

            MessageBox.Show("Transaction and products saved successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("Error while saving to database: " & ex.Message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            con.Close()
        End Try
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If dtOrders.Rows.Count = 0 Then
            MessageBox.Show("Your cart is empty. Add items before proceeding to payment.", "Empty Cart", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        SaveTransaction()
        NewTransaction()
        PAYMENT.Show()
        Me.Hide()
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Application.Exit()
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        STARTUP.Show()
        Me.Hide()
    End Sub
End Class
