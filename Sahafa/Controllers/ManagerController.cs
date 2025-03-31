using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sahafa.Data;
using Sahafa.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sahafa.Controllers
{
    public class ManagerController : Controller
    {
        private readonly string _connectionString;

        
        

        public ManagerController(IConfiguration configuration, ApplicationDbContext context)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        /*{
            View, Add, Update, Delete, Author
        }*/
        public IActionResult Authors()
        {
            List<Authors> authors = new List<Authors>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT AuthorID, FullName, DOB, Bio FROM Authors";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    authors.Add(new Authors
                    {
                        AuthorID = reader["AuthorID"].ToString(),
                        FullName = reader["FullName"]?.ToString() ?? "",
                        DOB = reader["DOB"] != DBNull.Value ? Convert.ToDateTime(reader["DOB"]) : (DateTime?)null,
                        Bio = reader["Bio"]?.ToString() ?? ""
                    });
                }
            }
            return View(authors);
        }

        [HttpPost]
        public IActionResult AddAuthor(Authors author)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Kiểm tra trùng lặp theo AuthorID (bạn có thể mở rộng kiểm tra theo FullName nếu cần)
                string checkQuery = "SELECT COUNT(*) FROM Authors WHERE AuthorID = @AuthorID";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@AuthorID", author.AuthorID);
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        TempData["ErrorMessage"] = "Mã tác giả đã tồn tại. Vui lòng chọn mã khác.";
                        return RedirectToAction("Authors");
                    }
                }

                string query = "INSERT INTO Authors (AuthorID, FullName, DOB, Bio) VALUES (@AuthorID, @FullName, @DOB, @Bio)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@AuthorID", author.AuthorID);
                    cmd.Parameters.AddWithValue("@FullName", author.FullName);
                    cmd.Parameters.AddWithValue("@DOB", (object)author.DOB ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Bio", (object)author.Bio ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Authors");
        }

        [HttpPost]
        public IActionResult UpdateAuthor(string oldID, string newID, string name, DateTime? dob, string bio)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Nếu thay đổi mã tác giả, kiểm tra trùng lặp
                        if (!string.IsNullOrEmpty(newID) && !oldID.Equals(newID, StringComparison.Ordinal))
                        {
                            string checkQuery = "SELECT COUNT(*) FROM Authors WHERE AuthorID = @NewID";
                            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@NewID", newID);
                                int count = (int)checkCmd.ExecuteScalar();
                                if (count > 0)
                                {
                                    TempData["ErrorMessage"] = "Mã tác giả mới đã tồn tại!";
                                    transaction.Rollback();
                                    return RedirectToAction("Authors");
                                }
                            }

                            // Cập nhật bảng Book nếu AuthorID thay đổi
                            string updateBooksQuery = "UPDATE Book SET AuthorID = @NewID WHERE AuthorID = @OldID";
                            using (SqlCommand updateBooksCmd = new SqlCommand(updateBooksQuery, conn, transaction))
                            {
                                updateBooksCmd.Parameters.AddWithValue("@NewID", newID);
                                updateBooksCmd.Parameters.AddWithValue("@OldID", oldID);
                                updateBooksCmd.ExecuteNonQuery();
                            }
                        }

                        // Cập nhật Authors (nếu không thay đổi thì newID sẽ là oldID)
                        string updateAuthorQuery = "UPDATE Authors SET AuthorID = @NewID, FullName = @Name, DOB = @DOB, Bio = @Bio WHERE AuthorID = @OldID";
                        using (SqlCommand cmd = new SqlCommand(updateAuthorQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@NewID", newID ?? oldID);
                            cmd.Parameters.AddWithValue("@Name", name);
                            cmd.Parameters.AddWithValue("@DOB", (object)dob ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Bio", (object)bio ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@OldID", oldID);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        TempData["ErrorMessage"] = "Lỗi khi cập nhật tác giả.";
                        return RedirectToAction("Authors");
                    }
                }
            }
            return RedirectToAction("Authors");
        }

        [HttpPost]
        public IActionResult DeleteAuthors(string id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Cập nhật bảng Book: set AuthorID = NULL
                        string updateBooksQuery = "UPDATE Book SET AuthorID = NULL WHERE AuthorID = @AuthorID";
                        using (SqlCommand updateBooksCmd = new SqlCommand(updateBooksQuery, conn, transaction))
                        {
                            updateBooksCmd.Parameters.AddWithValue("@AuthorID", id);
                            updateBooksCmd.ExecuteNonQuery();
                        }

                        // Xóa tác giả
                        string deleteQuery = "DELETE FROM Authors WHERE AuthorID = @AuthorID";
                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@AuthorID", id);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        TempData["ErrorMessage"] = "Lỗi khi xóa tác giả.";
                        return RedirectToAction("Authors");
                    }
                }
            }
            return RedirectToAction("Authors");
        }


        /*{
            View, Add, Update, Delete, Employee
        }*/

        public IActionResult Employees()
        {
            List<Employees> employees = new List<Employees>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT EmployeeID, FullName, Email, Phone, Address, DOB, EmployeeType FROM Employees";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    employees.Add(new Employees
                    {
                        EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                        FullName = reader["FullName"].ToString(),
                        Email = reader["Email"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        Address = reader["Address"].ToString(),
                        DOB = reader["DOB"] != DBNull.Value ? Convert.ToDateTime(reader["DOB"]) : (DateTime?)null,
                        EmployeeType = Convert.ToInt32(reader["EmployeeType"])
                    });
                }
            }
            return View(employees);
        }

        [HttpPost]
        public IActionResult AddEmployee(Employees employee)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO Employees (FullName, Email, Phone, Address, DOB, EmployeeType) VALUES (@FullName, @Email, @Phone, @Address, @DOB, @EmployeeType)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FullName", employee.FullName);
                cmd.Parameters.AddWithValue("@Email", employee.Email);
                cmd.Parameters.AddWithValue("@Phone", (object)employee.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", employee.Address);
                cmd.Parameters.AddWithValue("@DOB", (object)employee.DOB ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EmployeeType", employee.EmployeeType);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Employees");
        }

        [HttpPost]
        public IActionResult UpdateEmployee(int id, string fullName, string email, string phone, string address, DateTime? dob, int employeeType)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "UPDATE Employees SET FullName = @FullName, Email = @Email, Phone = @Phone, Address = @Address, DOB = @DOB, EmployeeType = @EmployeeType WHERE EmployeeID = @EmployeeID";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@EmployeeID", id);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone);
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@DOB", dob.HasValue ? (object)dob.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeType", employeeType);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { message = "Không tìm thấy nhân viên!" });
                    }
                }
                return RedirectToAction("Employees");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        [HttpPost]
        public IActionResult DeleteEmployee(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Employees WHERE EmployeeID = @EmployeeID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@EmployeeID", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Employees");
        }

        /*Ơ{
            View, Add, Update, Delete, Book
        }*/

        public IActionResult Book()
        {
            var list = new List<BookViewModel>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Lấy danh sách Tác giả
                string authorQuery = "SELECT AuthorID, FullName FROM Authors";
                SqlCommand authorCmd = new SqlCommand(authorQuery, conn);
                SqlDataReader authorReader = authorCmd.ExecuteReader();
                List<SelectListItem> authors = new List<SelectListItem>();
                while (authorReader.Read())
                {
                    authors.Add(new SelectListItem
                    {
                        Value = authorReader["AuthorID"].ToString(),
                        Text = authorReader["FullName"]?.ToString() ?? "Không có tên"
                    });
                }
                authorReader.Close();

                // Lấy danh sách Nhà cung cấp
                string supplierQuery = "SELECT SupplierID, SupplierName FROM Supplier";
                SqlCommand supplierCmd = new SqlCommand(supplierQuery, conn);
                SqlDataReader supplierReader = supplierCmd.ExecuteReader();
                List<SelectListItem> suppliers = new List<SelectListItem>();
                while (supplierReader.Read())
                {
                    suppliers.Add(new SelectListItem
                    {
                        Value = supplierReader["SupplierID"].ToString(),
                        Text = supplierReader["SupplierName"]?.ToString() ?? "Không xác định"
                    });
                }
                supplierReader.Close();

                // Lấy danh sách Thể loại
                string bookTypeQuery = "SELECT BookTypeID, TypeName FROM BookType";
                SqlCommand bookTypeCmd = new SqlCommand(bookTypeQuery, conn);
                SqlDataReader bookTypeReader = bookTypeCmd.ExecuteReader();
                List<SelectListItem> bookTypes = new List<SelectListItem>();
                while (bookTypeReader.Read())
                {
                    bookTypes.Add(new SelectListItem
                    {
                        Value = bookTypeReader["BookTypeID"].ToString(),
                        Text = bookTypeReader["TypeName"]?.ToString() ?? "Chưa có thể loại"
                    });
                }
                bookTypeReader.Close();

                // Lấy danh sách Sách với thể loại, trạng thái, publication year và description
                string bookQuery = @"
            SELECT b.BookID, b.Title, a.FullName AS AuthorName, s.SupplierName, 
                   b.Price, b.Image, b.Status, b.PublicationYear, b.Description,
                   STRING_AGG(bt.TypeName, ', ') AS BookTypes
            FROM Book b
            JOIN Authors a ON b.AuthorID = a.AuthorID
            JOIN Supplier s ON b.SupplierID = s.SupplierID
            LEFT JOIN Book_BookType bbt ON b.BookID = bbt.BookID
            LEFT JOIN BookType bt ON bbt.BookTypeID = bt.BookTypeID
            GROUP BY b.BookID, b.Title, a.FullName, s.SupplierName, b.Price, b.Image, b.Status, b.PublicationYear, b.Description";

                SqlCommand bookCmd = new SqlCommand(bookQuery, conn);
                SqlDataReader bookReader = bookCmd.ExecuteReader();
                List<BookViewModel> books = new List<BookViewModel>();
                while (bookReader.Read())
                {
                    books.Add(new BookViewModel
                    {
                        BookID = bookReader["BookID"].ToString(),
                        Title = bookReader["Title"]?.ToString() ?? "Chưa có tiêu đề",
                        AuthorName = bookReader["AuthorName"]?.ToString() ?? "Không rõ tác giả",
                        SupplierName = bookReader["SupplierName"]?.ToString() ?? "Không có nhà cung cấp",
                        Price = bookReader["Price"] != DBNull.Value ? Convert.ToDecimal(bookReader["Price"]) : 0,
                        Image = bookReader["Image"]?.ToString() ?? "no-image.png",
                        BookTypes = bookReader["BookTypes"] is DBNull || string.IsNullOrEmpty(bookReader["BookTypes"].ToString())
                                    ? new List<string>()
                                    : bookReader["BookTypes"].ToString().Split(',').Select(x => x.Trim()).ToList(),
                        Status = bookReader["Status"] != DBNull.Value ? Convert.ToInt32(bookReader["Status"]) : -1,
                        PublicationYear = bookReader["PublicationYear"] != DBNull.Value ? Convert.ToInt32(bookReader["PublicationYear"]) : 0,
                        Description = bookReader["Description"]?.ToString() ?? ""
                    });
                }
                bookReader.Close();

                // Truyền dữ liệu xuống View
                ViewBag.Authors = authors;
                ViewBag.Suppliers = suppliers;
                ViewBag.BookTypes = bookTypes;
                return View(books);
            }
        }

        [HttpPost]
        public IActionResult AddBook(Book book, IFormFile imageFile, List<int> bookTypeIDs)
        {
            string image = null;
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/image");
                Directory.CreateDirectory(uploadsFolder);
                image = $"/image/{Guid.NewGuid()}_{imageFile.FileName}";
                string filePath = Path.Combine(uploadsFolder, Path.GetFileName(image));
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(fileStream);
                }
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Book (BookID, Title, AuthorID, SupplierID, Price, Image, Status, PublicationYear, Description) " +
                               "VALUES (@BookID, @Title, @AuthorID, @SupplierID, @Price, @Image, @Status, @PublicationYear, @Description)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BookID", book.BookID);
                cmd.Parameters.AddWithValue("@Title", book.Title);
                cmd.Parameters.AddWithValue("@AuthorID", book.AuthorID);
                cmd.Parameters.AddWithValue("@SupplierID", book.SupplierID);
                cmd.Parameters.AddWithValue("@Price", book.Price);
                cmd.Parameters.AddWithValue("@Image", (object)image ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", book.Status);
                cmd.Parameters.AddWithValue("@PublicationYear", book.PublicationYear);
                cmd.Parameters.AddWithValue("@Description", book.Description ?? "");

                // Kiểm tra xem BookID đã tồn tại chưa
                string checkQuery = "SELECT COUNT(*) FROM Book WHERE BookID = @BookID";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@BookID", book.BookID);
                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    ModelState.AddModelError("BookID", "BookID đã tồn tại. Vui lòng chọn mã khác.");
                    return View(book);
                }

                cmd.ExecuteNonQuery();

                // Thêm thể loại vào bảng trung gian
                foreach (int bookTypeID in bookTypeIDs)
                {
                    string bookTypeQuery = "INSERT INTO Book_BookType (BookID, BookTypeID) VALUES (@BookID, @BookTypeID)";
                    SqlCommand bookTypeCmd = new SqlCommand(bookTypeQuery, conn);
                    bookTypeCmd.Parameters.AddWithValue("@BookID", book.BookID);
                    bookTypeCmd.Parameters.AddWithValue("@BookTypeID", bookTypeID);
                    bookTypeCmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Book");
        }

        [HttpPost]
        public IActionResult EditBook(
            string oldBookID,
            string newBookID,
            string title,
            string authorID,
            string supplierID,
            decimal price,
            IFormFile imageFile,
            List<int> bookTypeIDs,
            int status,
            int publicationYear,
            string description)
        {
            string image = null;
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/image");
                Directory.CreateDirectory(uploadsFolder);
                image = $"/image/{Guid.NewGuid()}_{imageFile.FileName}";
                string filePath = Path.Combine(uploadsFolder, Path.GetFileName(image));
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(fileStream);
                }
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        bool isBookIDChanged = !oldBookID.Equals(newBookID, StringComparison.Ordinal);
                        if (isBookIDChanged)
                        {
                            string checkNewIdQuery = "SELECT COUNT(*) FROM Book WHERE BookID = @NewBookID";
                            using (SqlCommand checkCmd = new SqlCommand(checkNewIdQuery, conn, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@NewBookID", newBookID);
                                int existed = (int)checkCmd.ExecuteScalar();
                                if (existed > 0)
                                {
                                    ModelState.AddModelError("newBookID", "BookID mới đã tồn tại!");
                                    transaction.Rollback();
                                    return RedirectToAction("Book");
                                }
                            }

                            string updateBBT = "UPDATE Book_BookType SET BookID = @NewBookID WHERE BookID = @OldBookID";
                            using (SqlCommand cmdBBT = new SqlCommand(updateBBT, conn, transaction))
                            {
                                cmdBBT.Parameters.AddWithValue("@NewBookID", newBookID);
                                cmdBBT.Parameters.AddWithValue("@OldBookID", oldBookID);
                                cmdBBT.ExecuteNonQuery();
                            }
                        }

                        string updateBookQuery = @"
                    UPDATE Book
                    SET BookID = @NewBookID,
                        Title = @Title,
                        AuthorID = @AuthorID,
                        SupplierID = @SupplierID,
                        Price = @Price,
                        Image = COALESCE(@Image, Image),
                        Status = @Status,
                        PublicationYear = @PublicationYear,
                        Description = @Description
                    WHERE BookID = @OldBookID
                ";

                        using (SqlCommand cmdUpdateBook = new SqlCommand(updateBookQuery, conn, transaction))
                        {
                            cmdUpdateBook.Parameters.AddWithValue("@NewBookID", newBookID);
                            cmdUpdateBook.Parameters.AddWithValue("@Title", title);
                            cmdUpdateBook.Parameters.AddWithValue("@AuthorID", authorID);
                            cmdUpdateBook.Parameters.AddWithValue("@SupplierID", supplierID);
                            cmdUpdateBook.Parameters.AddWithValue("@Price", price);
                            cmdUpdateBook.Parameters.AddWithValue("@Image", (object)image ?? DBNull.Value);
                            cmdUpdateBook.Parameters.AddWithValue("@Status", status);
                            cmdUpdateBook.Parameters.AddWithValue("@PublicationYear", publicationYear);
                            cmdUpdateBook.Parameters.AddWithValue("@Description", description ?? "");
                            cmdUpdateBook.Parameters.AddWithValue("@OldBookID", oldBookID);
                            cmdUpdateBook.ExecuteNonQuery();
                        }

                        string deleteBookTypeQuery = "DELETE FROM Book_BookType WHERE BookID = @BookID";
                        using (SqlCommand cmdDel = new SqlCommand(deleteBookTypeQuery, conn, transaction))
                        {
                            cmdDel.Parameters.AddWithValue("@BookID", newBookID);
                            cmdDel.ExecuteNonQuery();
                        }

                        if (bookTypeIDs != null && bookTypeIDs.Any())
                        {
                            string insertBBTQuery = "INSERT INTO Book_BookType (BookID, BookTypeID) VALUES (@BookID, @TypeID)";
                            foreach (var typeId in bookTypeIDs)
                            {
                                using (SqlCommand cmdIns = new SqlCommand(insertBBTQuery, conn, transaction))
                                {
                                    cmdIns.Parameters.AddWithValue("@BookID", newBookID);
                                    cmdIns.Parameters.AddWithValue("@TypeID", typeId);
                                    cmdIns.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            return RedirectToAction("Book");
        }

        [HttpPost]
        public IActionResult DeleteBook(string bookID)
        {
            string imagePath = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Lấy đường dẫn ảnh
                string selectImageQuery = "SELECT Image FROM Book WHERE BookID = @BookID";
                using (SqlCommand selectCmd = new SqlCommand(selectImageQuery, conn))
                {
                    selectCmd.Parameters.AddWithValue("@BookID", bookID);
                    imagePath = selectCmd.ExecuteScalar() as string;
                }

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string deleteBookTypeQuery = "DELETE FROM Book_BookType WHERE BookID = @BookID";
                        using (SqlCommand deleteBookTypeCmd = new SqlCommand(deleteBookTypeQuery, conn, transaction))
                        {
                            deleteBookTypeCmd.Parameters.AddWithValue("@BookID", bookID);
                            deleteBookTypeCmd.ExecuteNonQuery();
                        }

                        string deleteBookQuery = "DELETE FROM Book WHERE BookID = @BookID";
                        using (SqlCommand deleteBookCmd = new SqlCommand(deleteBookQuery, conn, transaction))
                        {
                            deleteBookCmd.Parameters.AddWithValue("@BookID", bookID);
                            deleteBookCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            if (!string.IsNullOrEmpty(imagePath))
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            return RedirectToAction("Book");
        }

        /*{
            View, Add, Update, Delete, Booktypes
        }*/

        // Read: Hiển thị danh sách BookType
        public IActionResult BookTypes()
        {
            List<BookType> bookTypes = new List<BookType>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT BookTypeID, TypeName FROM BookType";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bookTypes.Add(new BookType
                            {
                                // Giả sử BookTypeID là int và TypeName là string
                                BookTypeID = reader["BookTypeID"] != DBNull.Value ? Convert.ToInt32(reader["BookTypeID"]) : 0,
                                TypeName = reader["TypeName"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            return View(bookTypes);
        }

        [HttpPost]
        public IActionResult AddBookType(BookType bookType)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Kiểm tra xem đã có loại sách với tên tương tự chưa
                string checkQuery = "SELECT COUNT(*) FROM BookType WHERE TypeName = @TypeName";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@TypeName", bookType.TypeName);
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        // Nếu đã có, thêm thông báo lỗi và chuyển về view danh sách BookType
                        TempData["ErrorMessage"] = "Loại sách này đã tồn tại!";
                        return RedirectToAction("BookTypes");
                    }
                }

                // Nếu chưa trùng, thực hiện thêm mới
                string query = "INSERT INTO BookType (TypeName) VALUES (@TypeName)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TypeName", bookType.TypeName);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("BookTypes");
        }

        [HttpPost]
        public IActionResult UpdateBookType(int bookTypeID, string typeName)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Kiểm tra xem có bản ghi nào (khác bản ghi đang cập nhật) có cùng tên không
                string checkQuery = "SELECT COUNT(*) FROM BookType WHERE TypeName = @TypeName AND BookTypeID <> @BookTypeID";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@TypeName", typeName);
                    checkCmd.Parameters.AddWithValue("@BookTypeID", bookTypeID);
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        // Nếu có, hiển thị thông báo lỗi
                        TempData["ErrorMessage"] = "Loại sách này đã tồn tại!";
                        return RedirectToAction("BookTypes");
                    }
                }

                // Nếu chưa trùng, tiến hành cập nhật
                string updateQuery = "UPDATE BookType SET TypeName = @TypeName WHERE BookTypeID = @BookTypeID";
                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@TypeName", typeName);
                    cmd.Parameters.AddWithValue("@BookTypeID", bookTypeID);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("BookTypes");
        }


        [HttpPost]
        public IActionResult DeleteBookType(int bookTypeID)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Nếu bảng Book_BookType không có ON DELETE CASCADE, xóa các bản ghi liên quan trước
                string deleteRefQuery = "DELETE FROM Book_BookType WHERE BookTypeID = @BookTypeID";
                using (SqlCommand cmdRef = new SqlCommand(deleteRefQuery, conn))
                {
                    cmdRef.Parameters.AddWithValue("@BookTypeID", bookTypeID);
                    cmdRef.ExecuteNonQuery();
                }

                string query = "DELETE FROM BookType WHERE BookTypeID = @BookTypeID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BookTypeID", bookTypeID);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("BookTypes");
        }

        public IActionResult Suppliers()
        {
            List<Supplier> suppliers = new List<Supplier>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT SupplierID, SupplierName, Email, Phone, Address FROM Supplier";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        suppliers.Add(new Supplier
                        {
                            SupplierID = reader["SupplierID"].ToString(),
                            SupplierName = reader["SupplierName"]?.ToString() ?? "",
                            Email = reader["Email"]?.ToString() ?? "",
                            Phone = reader["Phone"]?.ToString() ?? "",
                            Address = reader["Address"]?.ToString() ?? ""
                        });
                    }
                }
            }
            return View(suppliers);
        }

        [HttpPost]
        public IActionResult AddSupplier(Supplier supplier)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Kiểm tra xem SupplierID, SupplierName hoặc Email đã tồn tại chưa
                string checkQuery = @"SELECT COUNT(*) FROM Supplier 
                                      WHERE SupplierID = @SupplierID 
                                      OR SupplierName = @SupplierName
                                      OR Email = @Email";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@SupplierID", supplier.SupplierID);
                    checkCmd.Parameters.AddWithValue("@SupplierName", supplier.SupplierName);
                    checkCmd.Parameters.AddWithValue("@Email", supplier.Email);
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        TempData["ErrorMessage"] = "SupplierID, SupplierName hoặc Email đã tồn tại. Vui lòng nhập giá trị khác.";
                        return RedirectToAction("Suppliers");
                    }
                }

                string query = @"INSERT INTO Supplier (SupplierID, SupplierName, Email, Phone, Address) 
                                 VALUES (@SupplierID, @SupplierName, @Email, @Phone, @Address)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SupplierID", supplier.SupplierID);
                    cmd.Parameters.AddWithValue("@SupplierName", supplier.SupplierName);
                    cmd.Parameters.AddWithValue("@Email", supplier.Email);
                    cmd.Parameters.AddWithValue("@Phone", supplier.Phone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", supplier.Address ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Suppliers");
        }

        [HttpPost]
        public IActionResult UpdateSupplier(string oldSupplierID, string newSupplierID, string supplierName, string email, string phone, string address)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        bool isSupplierIDChanged = !oldSupplierID.Equals(newSupplierID, StringComparison.Ordinal);

                        // Kiểm tra trùng lặp SupplierID, SupplierName hoặc Email (loại trừ bản ghi hiện tại)
                        string checkQuery = @"SELECT COUNT(*) FROM Supplier 
                                              WHERE (SupplierID = @NewSupplierID OR SupplierName = @SupplierName OR Email = @Email)
                                              AND SupplierID <> @OldSupplierID";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@NewSupplierID", newSupplierID);
                            checkCmd.Parameters.AddWithValue("@SupplierName", supplierName);
                            checkCmd.Parameters.AddWithValue("@Email", email);
                            checkCmd.Parameters.AddWithValue("@OldSupplierID", oldSupplierID);
                            int count = (int)checkCmd.ExecuteScalar();
                            if (count > 0)
                            {
                                TempData["ErrorMessage"] = "SupplierID, SupplierName hoặc Email đã tồn tại!";
                                transaction.Rollback();
                                return RedirectToAction("Suppliers");
                            }
                        }

                        if (isSupplierIDChanged)
                        {
                            // Cập nhật bảng Book
                            string updateBookQuery = "UPDATE Book SET SupplierID = @NewSupplierID WHERE SupplierID = @OldSupplierID";
                            using (SqlCommand updateBookCmd = new SqlCommand(updateBookQuery, conn, transaction))
                            {
                                updateBookCmd.Parameters.AddWithValue("@NewSupplierID", newSupplierID);
                                updateBookCmd.Parameters.AddWithValue("@OldSupplierID", oldSupplierID);
                                updateBookCmd.ExecuteNonQuery();
                            }
                            // Cập nhật bảng Stationery
                            string updateStationeryQuery = "UPDATE Stationery SET SupplierID = @NewSupplierID WHERE SupplierID = @OldSupplierID";
                            using (SqlCommand updateStationeryCmd = new SqlCommand(updateStationeryQuery, conn, transaction))
                            {
                                updateStationeryCmd.Parameters.AddWithValue("@NewSupplierID", newSupplierID);
                                updateStationeryCmd.Parameters.AddWithValue("@OldSupplierID", oldSupplierID);
                                updateStationeryCmd.ExecuteNonQuery();
                            }
                        }

                        // Cập nhật Supplier
                        string updateSupplierQuery = @"UPDATE Supplier 
                                                       SET SupplierID = @NewSupplierID,
                                                           SupplierName = @SupplierName,
                                                           Email = @Email,
                                                           Phone = @Phone,
                                                           Address = @Address
                                                       WHERE SupplierID = @OldSupplierID";
                        using (SqlCommand cmd = new SqlCommand(updateSupplierQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@NewSupplierID", newSupplierID);
                            cmd.Parameters.AddWithValue("@SupplierName", supplierName);
                            cmd.Parameters.AddWithValue("@Email", email);
                            cmd.Parameters.AddWithValue("@Phone", phone ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Address", address ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@OldSupplierID", oldSupplierID);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        TempData["ErrorMessage"] = "Lỗi khi cập nhật Supplier.";
                        return RedirectToAction("Suppliers");
                    }
                }
            }
            return RedirectToAction("Suppliers");
        }

        [HttpPost]
        public IActionResult DeleteSupplier(string supplierID)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Cập nhật bảng Book: set SupplierID = NULL
                        string updateBookQuery = "UPDATE Book SET SupplierID = NULL WHERE SupplierID = @SupplierID";
                        using (SqlCommand updateBookCmd = new SqlCommand(updateBookQuery, conn, transaction))
                        {
                            updateBookCmd.Parameters.AddWithValue("@SupplierID", supplierID);
                            updateBookCmd.ExecuteNonQuery();
                        }
                        // Cập nhật bảng Stationery: set SupplierID = NULL
                        string updateStationeryQuery = "UPDATE Stationery SET SupplierID = NULL WHERE SupplierID = @SupplierID";
                        using (SqlCommand updateStationeryCmd = new SqlCommand(updateStationeryQuery, conn, transaction))
                        {
                            updateStationeryCmd.Parameters.AddWithValue("@SupplierID", supplierID);
                            updateStationeryCmd.ExecuteNonQuery();
                        }
                        // Xóa Supplier
                        string deleteSupplierQuery = "DELETE FROM Supplier WHERE SupplierID = @SupplierID";
                        using (SqlCommand deleteCmd = new SqlCommand(deleteSupplierQuery, conn, transaction))
                        {
                            deleteCmd.Parameters.AddWithValue("@SupplierID", supplierID);
                            deleteCmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        TempData["ErrorMessage"] = "Lỗi khi xóa Supplier.";
                        return RedirectToAction("Suppliers");
                    }
                }
            }
            return RedirectToAction("Suppliers");
        }

        public IActionResult Stationeries()
        {
            List<Stationery> stationeries = new List<Stationery>();
            List<SelectListItem> suppliers = new List<SelectListItem>();
            List<SelectListItem> types = new List<SelectListItem>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // 1. Lấy danh sách Supplier
                string supplierQuery = "SELECT SupplierID, SupplierName FROM Supplier";
                using (SqlCommand supCmd = new SqlCommand(supplierQuery, conn))
                {
                    using (SqlDataReader reader = supCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            suppliers.Add(new SelectListItem
                            {
                                Value = reader["SupplierID"].ToString(),
                                Text = reader["SupplierName"].ToString()
                            });
                        }
                    }
                }

                // 2. Lấy danh sách StationeryType
                string typeQuery = "SELECT TypeID, TypeName FROM StationeryType";
                using (SqlCommand typeCmd = new SqlCommand(typeQuery, conn))
                {
                    using (SqlDataReader reader = typeCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            types.Add(new SelectListItem
                            {
                                Value = reader["TypeID"].ToString(),
                                Text = reader["TypeName"].ToString()
                            });
                        }
                    }
                }

                // 3. Lấy danh sách Stationery (join Supplier, StationeryType)
                string stationeryQuery = @"
                SELECT st.StationeryID, st.Name, st.SupplierID, st.Price, st.Status, st.TypeID, st.Image,
                       sp.SupplierName,
                       sty.TypeName
                FROM Stationery st
                LEFT JOIN Supplier sp ON st.SupplierID = sp.SupplierID
                LEFT JOIN StationeryType sty ON st.TypeID = sty.TypeID
            ";
                using (SqlCommand stCmd = new SqlCommand(stationeryQuery, conn))
                {
                    using (SqlDataReader reader = stCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stationeries.Add(new Stationery
                            {
                                StationeryID = reader["StationeryID"].ToString(),
                                Name = reader["Name"].ToString(),
                                SupplierID = reader["SupplierID"]?.ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                Status = Convert.ToInt32(reader["Status"]),
                                TypeID = reader["TypeID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["TypeID"]),
                                Image = reader["Image"]?.ToString(),
                                SupplierName = reader["SupplierName"]?.ToString(),
                                TypeName = reader["TypeName"]?.ToString()
                            });
                        }
                    }
                }
            }

            ViewBag.Suppliers = suppliers;
            ViewBag.StationeryTypes = types;
            return View(stationeries);
        }


        // CREATE: Thêm mới Stationery
        [HttpPost]
        public IActionResult AddStationery(Stationery stationery, IFormFile imageFile)
        {
            // 1. Lưu ảnh (nếu có)
            string imagePath = null;
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/image");
                Directory.CreateDirectory(uploadsFolder);
                // Tạo tên file unique
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                imagePath = "/image/" + uniqueFileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(fileStream);
                }
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Kiểm tra StationeryID trùng
                string checkQuery = "SELECT COUNT(*) FROM Stationery WHERE StationeryID = @ID";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@ID", stationery.StationeryID);
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        TempData["ErrorMessage"] = "StationeryID đã tồn tại. Vui lòng chọn mã khác.";
                        return RedirectToAction("Stationeries");
                    }
                }

                string insertQuery = @"
                INSERT INTO Stationery (StationeryID, Name, SupplierID, Price, Status, TypeID, Image)
                VALUES (@ID, @Name, @SupplierID, @Price, @Status, @TypeID, @Image)
            ";
                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", stationery.StationeryID);
                    cmd.Parameters.AddWithValue("@Name", stationery.Name);
                    cmd.Parameters.AddWithValue("@SupplierID", (object)stationery.SupplierID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Price", stationery.Price);
                    cmd.Parameters.AddWithValue("@Status", stationery.Status);
                    cmd.Parameters.AddWithValue("@TypeID", (object)stationery.TypeID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Image", (object)imagePath ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Stationeries");
        }

        // UPDATE: Sửa Stationery
        [HttpPost]
        public IActionResult UpdateStationery(
            string oldID,
            string newID,
            string name,
            string supplierID,
            decimal price,
            int status,
            int? TypeID,
            IFormFile imageFile
        )
        {
            string imagePath = null;
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/image");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                imagePath = "/image/" + uniqueFileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(fileStream);
                }
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        bool isIDChanged = !oldID.Equals(newID, StringComparison.Ordinal);
                        if (isIDChanged)
                        {
                            // Kiểm tra newID đã tồn tại chưa
                            string checkQuery = "SELECT COUNT(*) FROM Stationery WHERE StationeryID = @newID";
                            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@newID", newID);
                                int count = (int)checkCmd.ExecuteScalar();
                                if (count > 0)
                                {
                                    TempData["ErrorMessage"] = "StationeryID mới đã tồn tại!";
                                    transaction.Rollback();
                                    return RedirectToAction("Stationeries");
                                }
                            }
                            // Update ID ở chính bảng Stationery
                            string updateIDQuery = "UPDATE Stationery SET StationeryID = @newID WHERE StationeryID = @oldID";
                            using (SqlCommand updateIDCmd = new SqlCommand(updateIDQuery, conn, transaction))
                            {
                                updateIDCmd.Parameters.AddWithValue("@newID", newID);
                                updateIDCmd.Parameters.AddWithValue("@oldID", oldID);
                                updateIDCmd.ExecuteNonQuery();
                            }
                        }

                        // Lấy ảnh cũ (nếu có) để xóa nếu cần
                        string oldImage = null;
                        {
                            string selectImageQuery = "SELECT Image FROM Stationery WHERE StationeryID = @ID";
                            using (SqlCommand selCmd = new SqlCommand(selectImageQuery, conn, transaction))
                            {
                                selCmd.Parameters.AddWithValue("@ID", isIDChanged ? newID : oldID);
                                oldImage = selCmd.ExecuteScalar() as string;
                            }
                        }

                        // Update các trường còn lại
                        string updateQuery = @"
                        UPDATE Stationery
                        SET Name = @Name,
                            SupplierID = @SupplierID,
                            Price = @Price,
                            Status = @Status,
                            TypeID = @TypeID,
                            Image = COALESCE(@Image, Image)
                        WHERE StationeryID = @ID
                    ";
                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Name", name);
                            cmd.Parameters.AddWithValue("@SupplierID", (object)supplierID ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Price", price);
                            cmd.Parameters.AddWithValue("@Status", status);
                            cmd.Parameters.AddWithValue("@TypeID", (object)TypeID ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Image", (object)imagePath ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ID", isIDChanged ? newID : oldID);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        // Nếu có ảnh cũ và có ảnh mới, có thể xóa ảnh cũ
                        // (Chỉ xóa nếu thay ảnh, còn nếu user không upload thì giữ nguyên)
                        if (!string.IsNullOrEmpty(imagePath) && !string.IsNullOrEmpty(oldImage))
                        {
                            // Xóa file cũ
                            string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImage.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            return RedirectToAction("Stationeries");
        }

        // DELETE: Xóa Stationery (xóa cả ảnh)
        [HttpPost]
        public IActionResult DeleteStationery(string stationeryID)
        {
            string oldImage = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Lấy đường dẫn ảnh cũ
                string selectQuery = "SELECT Image FROM Stationery WHERE StationeryID = @ID";
                using (SqlCommand selCmd = new SqlCommand(selectQuery, conn))
                {
                    selCmd.Parameters.AddWithValue("@ID", stationeryID);
                    oldImage = selCmd.ExecuteScalar() as string;
                }

                // Xóa Stationery
                string deleteQuery = "DELETE FROM Stationery WHERE StationeryID = @ID";
                using (SqlCommand delCmd = new SqlCommand(deleteQuery, conn))
                {
                    delCmd.Parameters.AddWithValue("@ID", stationeryID);
                    delCmd.ExecuteNonQuery();
                }
            }

            // Xóa file ảnh (nếu có)
            if (!string.IsNullOrEmpty(oldImage))
            {
                string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImage.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            return RedirectToAction("Stationeries");
        }
    }
}
