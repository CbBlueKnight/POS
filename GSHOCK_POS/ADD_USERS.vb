Imports System.Data.SqlClient
Imports System.Drawing
Imports System.IO
Imports ImageMagick

Public Class ADD_USERS
    Dim connectionString As String = "Data Source=DESKTOP-UD7BN0F;Initial Catalog=gshock;Integrated Security=True"

    Private Sub ClearFields()
        txtUsername.Text = ""
        txtName.Text = ""
        cmbPosition.SelectedIndex = -1
        txtPassword.Text = ""
        picProfile.Image = Nothing
    End Sub

    Private Sub LoadPositionDropdown()
        cmbPosition.Items.Clear()
        cmbPosition.Items.AddRange(New String() {"Cashier", "Manager", "Admin"})
    End Sub

    Private Sub ADD_USERS_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadPositionDropdown()

        picProfile.SizeMode = PictureBoxSizeMode.Zoom
        picProfile.Width = 80
        picProfile.Height = 80

        FlowLayoutPanel1.WrapContents = True
        FlowLayoutPanel1.AutoScroll = True
        FlowLayoutPanel1.AutoSize = False
        FlowLayoutPanel1.Width = 640
        FlowLayoutPanel1.Height = 600

        LoadUsers()
        LoadUserTable()
    End Sub

    Private Sub LoadUsers()
        FlowLayoutPanel1.Controls.Clear()

        Using conn As New SqlConnection(connectionString)
            Dim cmd = New SqlCommand("SELECT * FROM login", conn)
            conn.Open()
            Using reader = cmd.ExecuteReader()
                While reader.Read()
                    Dim pnl As New Panel With {
                        .Width = 200,
                        .Height = 280,
                        .BackColor = Color.LightGray,
                        .Margin = New Padding(5)
                    }

                    Dim pic As New PictureBox With {
                        .Width = 100,
                        .Height = 100,
                        .SizeMode = PictureBoxSizeMode.Zoom,
                        .Top = 10,
                        .Left = 50
                    }

                    If Not IsDBNull(reader("image")) AndAlso Not String.IsNullOrEmpty(reader("image").ToString()) Then
                        Try
                            Dim imgBytes = Convert.FromBase64String(reader("image").ToString())
                            Using ms As New MemoryStream(imgBytes)
                                pic.Image = Image.FromStream(ms)
                            End Using
                        Catch
                            pic.Image = Nothing
                        End Try
                    End If

                    Dim lblName As New Label With {
                        .Text = "Name: " & reader("Name").ToString(),
                        .Top = 120,
                        .Left = 10,
                        .Width = 180,
                        .TextAlign = ContentAlignment.MiddleCenter
                    }

                    Dim lblPos As New Label With {
                        .Text = "Position: " & reader("Position").ToString(),
                        .Top = 150,
                        .Left = 10,
                        .Width = 180,
                        .TextAlign = ContentAlignment.MiddleCenter
                    }

                    Dim btnEdit As New Button With {
                        .Text = "Edit",
                        .Top = 200,
                        .Left = 60,
                        .Width = 80
                    }

                    Dim username As String = reader("Username").ToString()
                    AddHandler btnEdit.Click, Sub(s, eArgs)
                                                  Dim editForm As New EditUserForm(username)
                                                  editForm.ShowDialog()
                                                  LoadUsers()
                                                  LoadUserTable()
                                              End Sub

                    pnl.Controls.Add(pic)
                    pnl.Controls.Add(lblName)
                    pnl.Controls.Add(lblPos)
                    pnl.Controls.Add(btnEdit)

                    FlowLayoutPanel1.Controls.Add(pnl)
                End While
            End Using
        End Using
    End Sub

    Private Sub LoadUserTable()
        Using conn As New SqlConnection(connectionString)
            Dim cmd As New SqlCommand("SELECT Username, Name, Position FROM login", conn)
            Dim adapter As New SqlDataAdapter(cmd)
            Dim table As New DataTable()
            adapter.Fill(table)
            DataGridView1.DataSource = table
        End Using
    End Sub

    Private Sub btnBrowseImage_Click(sender As Object, e As EventArgs) Handles btnBrowseImage.Click
        Using ofd As New OpenFileDialog()
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            If ofd.ShowDialog() = DialogResult.OK Then
                picProfile.Image = Image.FromFile(ofd.FileName)
            End If
        End Using
    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        Dim username = txtUsername.Text.Trim()
        Dim name = txtName.Text.Trim()
        Dim position = If(cmbPosition.SelectedItem?.ToString(), "")
        Dim password = txtPassword.Text.Trim()

        If username = "" OrElse name = "" OrElse position = "" OrElse password = "" Then
            MessageBox.Show("Please fill all fields.")
            Return
        End If

        Dim base64Image As String = ""
        If picProfile.Image IsNot Nothing Then
            Using ms As New MemoryStream()
                picProfile.Image.Save(ms, Imaging.ImageFormat.Png)
                base64Image = Convert.ToBase64String(ms.ToArray())
            End Using
        End If

        Using conn As New SqlConnection(connectionString)
            Dim cmd = New SqlCommand("INSERT INTO login (Username, Password, Name, Position, image) VALUES (@Username, @Password, @Name, @Position, @ProfileImage)", conn)
            cmd.Parameters.AddWithValue("@Username", username)
            cmd.Parameters.AddWithValue("@Password", password)
            cmd.Parameters.AddWithValue("@Name", name)
            cmd.Parameters.AddWithValue("@Position", position)
            cmd.Parameters.AddWithValue("@ProfileImage", base64Image)
            conn.Open()
            cmd.ExecuteNonQuery()
        End Using

        LoadUsers()
        LoadUserTable()
        ClearFields()
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As EventArgs) Handles btnEdit.Click
        Dim username = txtUsername.Text.Trim()
        Dim name = txtName.Text.Trim()
        Dim position = If(cmbPosition.SelectedItem?.ToString(), "")
        Dim password = txtPassword.Text.Trim()

        If username = "" Then
            MessageBox.Show("Please select a user to edit.")
            Return
        End If

        Dim base64Image As String = ""
        If picProfile.Image IsNot Nothing Then
            Using ms As New MemoryStream()
                picProfile.Image.Save(ms, Imaging.ImageFormat.Png)
                base64Image = Convert.ToBase64String(ms.ToArray())
            End Using
        End If

        Using conn As New SqlConnection(connectionString)
            Dim cmd = New SqlCommand("UPDATE login SET Password=@Password, Name=@Name, Position=@Position, image=@image WHERE Username=@Username", conn)
            cmd.Parameters.AddWithValue("@Password", password)
            cmd.Parameters.AddWithValue("@Name", name)
            cmd.Parameters.AddWithValue("@Position", position)
            cmd.Parameters.AddWithValue("@image", base64Image)
            cmd.Parameters.AddWithValue("@Username", username)
            conn.Open()
            cmd.ExecuteNonQuery()
        End Using

        LoadUsers()
        LoadUserTable()
        ClearFields()
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        Dim username = txtUsername.Text.Trim()

        If username = "" Then
            MessageBox.Show("Please select a user to delete.")
            Return
        End If

        If MessageBox.Show("Delete this user?", "Confirm", MessageBoxButtons.YesNo) = DialogResult.Yes Then
            Using conn As New SqlConnection(connectionString)
                Dim cmd = New SqlCommand("DELETE FROM login WHERE Username=@Username", conn)
                cmd.Parameters.AddWithValue("@Username", username)
                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End If

        LoadUsers()
        LoadUserTable()
        ClearFields()
    End Sub

    Private Sub DataGridView1_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellClick
        If e.RowIndex >= 0 Then
            Dim row As DataGridViewRow = DataGridView1.Rows(e.RowIndex)
            txtUsername.Text = row.Cells("Username").Value.ToString()
            txtName.Text = row.Cells("Name").Value.ToString()
            cmbPosition.SelectedItem = row.Cells("Position").Value.ToString()

            Using conn As New SqlConnection(connectionString)
                Dim cmd As New SqlCommand("SELECT Password, image FROM login WHERE Username=@Username", conn)
                cmd.Parameters.AddWithValue("@Username", txtUsername.Text)
                conn.Open()
                Using reader = cmd.ExecuteReader()
                    If reader.Read() Then
                        txtPassword.Text = reader("Password").ToString()
                        If Not IsDBNull(reader("image")) AndAlso Not String.IsNullOrEmpty(reader("image").ToString()) Then
                            Dim imgBytes = Convert.FromBase64String(reader("image").ToString())
                            Using ms As New MemoryStream(imgBytes)
                                picProfile.Image = Image.FromStream(ms)
                            End Using
                        Else
                            picProfile.Image = Nothing
                        End If
                    End If
                End Using
            End Using
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        MANAGER_SALES_OVERVIEW.Show()
        Me.Hide()
    End Sub
End Class
