Imports System.Data.SqlClient
Imports System.IO
Imports System.Windows.Forms
Imports ImageMagick

Public Class PRODUCT_WEB

    Dim imageBytes As Byte()
    Dim imageFilePath As String
    Dim base64String As String
    Dim imageName As String

    Private Sub PRODUCT_WEB_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        cmbSeries.Items.AddRange(New String() {"women", "men", "kids"})
        cmbSeries.DropDownStyle = ComboBoxStyle.DropDownList
    End Sub

    Private Sub btnSelectImage_Click(sender As Object, e As EventArgs) Handles btnSelectImage.Click
        Dim openFileDialog As New OpenFileDialog()
        openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.avif"
        openFileDialog.Title = "Select an Image"

        If openFileDialog.ShowDialog() = DialogResult.OK Then
            Dim imagePath As String = openFileDialog.FileName
            Dim extension As String = Path.GetExtension(imagePath).ToLower()
            imageName = Path.GetFileName(imagePath)
            base64String = Convert.ToBase64String(File.ReadAllBytes(imagePath))

            Dim uniqueImageName As String = Guid.NewGuid().ToString() & extension
            imageFilePath = Path.Combine("C:\Users\Lenovo\Desktop\FRONT-END\img", uniqueImageName)

            File.Copy(imagePath, imageFilePath, True)

            If extension = ".avif" Then
                Using image As New MagickImage(imagePath)
                    image.Format = MagickFormat.Bmp
                    Dim bitmap As Bitmap = New Bitmap(New MemoryStream(image.ToByteArray()))
                    picProductImage.SizeMode = PictureBoxSizeMode.Zoom
                    picProductImage.Image = bitmap
                    imageBytes = File.ReadAllBytes(imagePath)
                End Using
            Else
                imageBytes = File.ReadAllBytes(imagePath)
                picProductImage.SizeMode = PictureBoxSizeMode.Zoom
                picProductImage.Image = Image.FromFile(imagePath)
            End If
        End If
    End Sub

    Private Sub btnInsertProduct_Click(sender As Object, e As EventArgs) Handles btnInsertProduct.Click
        If String.IsNullOrWhiteSpace(txtId.Text) OrElse
       String.IsNullOrWhiteSpace(txtProductName.Text) OrElse
       cmbSeries.SelectedItem Is Nothing OrElse
       String.IsNullOrWhiteSpace(txtPrice.Text) OrElse
       String.IsNullOrWhiteSpace(txtQuantity.Text) OrElse
       String.IsNullOrWhiteSpace(base64String) Then
            MessageBox.Show("Please fill in all fields and select an image.")
            Return
        End If

        Dim priceValue As Decimal
        If Not Decimal.TryParse(txtPrice.Text, priceValue) Then
            MessageBox.Show("Invalid price format. Use numbers like 99.99.")
            Return
        End If

        Dim quantityValue As Integer
        If Not Integer.TryParse(txtQuantity.Text, quantityValue) Then
            MessageBox.Show("Invalid quantity. Use whole numbers only.")
            Return
        End If

        Try
            Dim productConnectionString As String = "Data Source=DESKTOP-UD7BN0F;Initial Catalog=gshock;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
            Dim productQuery As String = "
            IF EXISTS (SELECT 1 FROM products WHERE id = @id)
                UPDATE products 
                SET productname = @productname, 
                    series = @series, 
                    price = @price, 
                    quantity = @quantity, 
                    image = @image
                WHERE id = @id
            ELSE
                INSERT INTO products (id, productname, series, price, quantity, image)
                VALUES (@id, @productname, @series, @price, @quantity, @image)
        "

            Using connection As New SqlConnection(productConnectionString)
                Using command As New SqlCommand(productQuery, connection)
                    command.Parameters.AddWithValue("@id", txtId.Text)
                    command.Parameters.AddWithValue("@productname", txtProductName.Text)
                    command.Parameters.AddWithValue("@series", cmbSeries.SelectedItem.ToString())
                    command.Parameters.AddWithValue("@price", priceValue)
                    command.Parameters.AddWithValue("@quantity", quantityValue)
                    command.Parameters.AddWithValue("@image", base64String)

                    connection.Open()
                    command.ExecuteNonQuery()
                End Using
            End Using

            MessageBox.Show("Product saved successfully with Base64 image.")

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message)
        End Try
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        MANAGER_SALES_OVERVIEW.Show()
        Me.Hide()
    End Sub
End Class
