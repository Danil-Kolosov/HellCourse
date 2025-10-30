using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FamilyTree;
using System;
using System.Windows.Forms;
using GenealogySystem;
namespace FamilyTree
{
    public partial class MainForm : Form
    {
        private PersonRepository _personRepo;
        private FamilyConnectionRepository _connectionRepo;

        public MainForm()
        {
            InitializeComponent();
            _personRepo = new PersonRepository();
            _connectionRepo = new FamilyConnectionRepository(_personRepo);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadData();
            UpdateStatus("Данные загружены");
        }

        private void LoadData()
        {
            try
            {
                DataSaver.LoadAllData(_personRepo, _connectionRepo);

                // Привязка данных к таблицам
                dgvPersons.DataSource = _personRepo.GetAllPersons();
                dgvConnections.DataSource = _connectionRepo.GetAllConnections();

                // Настройка столбцов для персон
                if (dgvPersons.Columns.Count > 0)
                {
                    dgvPersons.Columns["PersonId"].HeaderText = "ID";
                    dgvPersons.Columns["Surname"].HeaderText = "Фамилия";
                    dgvPersons.Columns["Name"].HeaderText = "Имя";
                    dgvPersons.Columns["LastName"].HeaderText = "Отчество";
                    //dgvPersons.Columns["GenderId"].HeaderText = "Пол";
                    dgvPersons.Columns["GenderName"].HeaderText = "Пол";
                    dgvPersons.Columns["LastModifiedDate"].HeaderText = "Последняя дата изменения";
                    dgvPersons.Columns["GenderId"].Visible = false;
                    dgvPersons.Columns["BirthDate"].HeaderText = "Дата рождения";
                    dgvPersons.Columns["DeathDate"].HeaderText = "Дата смерти";
                    dgvPersons.Columns["Biography"].HeaderText = "Биография";
                }

                // Настройка столбцов для связей
                if (dgvConnections.Columns.Count > 0)
                {
                    dgvConnections.Columns["PersonId1"].HeaderText = "ID Персоны 1";
                    dgvConnections.Columns["PersonId2"].HeaderText = "ID Персоны 2";
                    dgvConnections.Columns["ConnectionTypeId"].HeaderText = "Тип связи";
                    dgvConnections.Columns["LastModifiedDate"].HeaderText = "Последняя дата изменения";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
            UpdateStatus("Данные обновлены");
        }

        // ===== МЕНЮ ДОБАВИТЬ =====
        private void добавитьПерсонуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new AddPersonForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _personRepo.Add(form.Person);
                        DataSaver.SaveAllData(_personRepo, _connectionRepo);
                        LoadData();
                        UpdateStatus("Персона добавлена");
                        MessageBox.Show("Персона успешно добавлена!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void добавитьСвязьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((_personRepo.GetAllPersons()).Count < 2)
            {
                MessageBox.Show("Для создания связи нужно как минимум 2 персоны", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var form = new AddConnectionForm(_personRepo))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _connectionRepo.Add(form.Connection);
                        DataSaver.SaveAllData(_personRepo, _connectionRepo);
                        LoadData();
                        UpdateStatus("Связь добавлена");
                        MessageBox.Show("Родственная связь успешно добавлена!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ===== МЕНЮ ПОЛУЧИТЬ =====
        private void всеПерсоныToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPagePersons;
            UpdateStatus("Отображены все персоны");
        }

        private void всеСвязиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPageConnections;
            UpdateStatus("Отображены все связи");
        }

        private void найтиПерсонуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new SearchPersonForm(_personRepo))
            {
                form.ShowDialog();
            }
        }

        private void найтиСвязьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new SearchConnectionForm(_connectionRepo, _personRepo))
            {
                form.ShowDialog();
            }
        }

        // ===== МЕНЮ ИЗМЕНИТЬ =====
        private void изменитьПерсонуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Сначала поиск персоны
            using (var searchForm = new SearchPersonForm(_personRepo))
            {
                searchForm.Text = "Поиск персоны для редактирования";
                if (searchForm.ShowDialog() == DialogResult.OK && searchForm.SelectedPerson != null)
                {
                    // Потом редактирование выбранной персоны
                    EditSelectedPerson(searchForm.SelectedPerson);
                }
            }
        }
        private void изменитьРодственнуюСвязьToolStripMenuItem_Click(object sender, EventArgs e) 
        {
            // Сначала поиск связи
            using (var searchForm = new SearchConnectionForm(_connectionRepo, _personRepo))
            {
                searchForm.Text = "Поиск связи для редактирования";
                if (searchForm.ShowDialog() == DialogResult.OK && searchForm.SelectedConnection != null)
                {
                    // Потом редактирование выбранной связи
                    EditSelectedConnection(searchForm.SelectedConnection);
                }
            }
        }

        // Вынесем логику редактирования в отдельные методы
        private void EditSelectedPerson(Person selectedPerson)
        {
            using (var editForm = new EditPersonForm(selectedPerson))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var allPersons = _personRepo.GetAllPersons();
                        var index = allPersons.FindIndex(p => p.PersonId == selectedPerson.PersonId);

                        if (index >= 0)
                        {
                            _personRepo.Update(index,
                                selectedPerson.PersonId, // Используем исходный ID
                                editForm.UpdatedPerson.Surname,
                                editForm.UpdatedPerson.Name,
                                editForm.UpdatedPerson.LastName,
                                editForm.UpdatedPerson.GenderId,
                                editForm.UpdatedPerson.BirthDate,
                                editForm.UpdatedPerson.DeathDate,
                                editForm.UpdatedPerson.Biography
                            );

                            DataSaver.SaveAllData(_personRepo, _connectionRepo);
                            LoadData();
                            UpdateStatus("Персона изменена");
                            MessageBox.Show("Данные персоны успешно обновлены!", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void EditSelectedConnection(FamilyConnection selectedConnection)
        {
            using (var editForm = new EditConnectionForm(selectedConnection, _personRepo))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var allConnections = _connectionRepo.GetAllConnections();
                        var index = allConnections.FindIndex(c =>
                            c.PersonId1 == selectedConnection.PersonId1 &&
                            c.PersonId2 == selectedConnection.PersonId2 &&
                            c.ConnectionTypeId == selectedConnection.ConnectionTypeId);

                        if (index >= 0)
                        {
                            _connectionRepo.Update(index,
                                editForm.UpdatedConnection.PersonId1,
                                editForm.UpdatedConnection.PersonId2,
                                editForm.UpdatedConnection.ConnectionTypeId
                            );

                            DataSaver.SaveAllData(_personRepo, _connectionRepo);
                            LoadData();
                            UpdateStatus("Связь изменена");
                            MessageBox.Show("Связь успешно обновлена!", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        // ===== МЕНЮ УДАЛИТЬ =====
        //private void удалитьПерсонуToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (dgvPersons.CurrentRow?.DataBoundItem is Person selectedPerson)
        //    {
        //        var result = MessageBox.Show(
        //            $"Вы уверены, что хотите удалить персону: {selectedPerson.Surname} {selectedPerson.Name}?",
        //            "Подтверждение удаления",
        //            MessageBoxButtons.YesNo,
        //            MessageBoxIcon.Question);

        //        if (result == DialogResult.Yes)
        //        {
        //            try
        //            {
        //                var allPersons = _personRepo.GetAllPersons();
        //                var index = allPersons.FindIndex(p => p.PersonId == selectedPerson.PersonId);

        //                if (index >= 0)
        //                {
        //                    _personRepo.Remove(index);
        //                    DataSaver.SaveAllData(_personRepo, _connectionRepo);
        //                    LoadData();
        //                    UpdateStatus("Персона удалена");
        //                    MessageBox.Show("Персона успешно удалена!", "Успех",
        //                        MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
        //                    MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("Выберите персону для удаления", "Внимание",
        //            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    }
        //}

        private void удалитьСвязьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Сначала поиск связи
            using (var searchForm = new SearchConnectionForm(_connectionRepo, _personRepo))
            {
                searchForm.Text = "Поиск связи для удаления";
                if (searchForm.ShowDialog() == DialogResult.OK && searchForm.SelectedConnection != null)
                {
                    // Подтверждение удаления
                    var result = MessageBox.Show(
                        "Вы уверены, что хотите удалить эту связь?",
                        "Подтверждение удаления",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        DeleteSelectedConnection(searchForm.SelectedConnection);
                    }
                }
            }
        }

        // Метод для удаления связи
        private void DeleteSelectedConnection(FamilyConnection selectedConnection)
        {
            try
            {
                var allConnections = _connectionRepo.GetAllConnections();
                var index = allConnections.FindIndex(c =>
                    c.PersonId1 == selectedConnection.PersonId1 &&
                    c.PersonId2 == selectedConnection.PersonId2 &&
                    c.ConnectionTypeId == selectedConnection.ConnectionTypeId);

                if (index >= 0)
                {
                    _connectionRepo.Remove(index);
                    DataSaver.SaveAllData(_personRepo, _connectionRepo);
                    LoadData();
                    UpdateStatus("Связь удалена");
                    MessageBox.Show("Связь успешно удалена!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Связь не найдена в репозитории", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatus(string message)
        {
            lblStatus.Text = message;
        }
    }

}
