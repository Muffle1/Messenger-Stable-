using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Messenger
{
    public class ChatViewModel : BaseViewModel
    {
        public RelayCommand PopupCommand { get; private set; }
        public RelayCommand LeaveChatCommand { get; private set; }
        public RelayCommand SendMessageCommand { get; private set; }
        public RelayCommand SaveFileCommand { get; private set; }
        public RelayCommand AddFileCommand { get; private set; }
        public RelayCommand RemoveFileCommand { get; private set; }
        public RelayCommand CallCommand { get; private set; }

        public static event Action ScrollToBottom;

        private Message _message;
        public Message Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        private Chat _selectedChat;
        public Chat SelectedChat
        {
            get => _selectedChat;
            set
            {
                _selectedChat = value;
                OnPropertyChanged(nameof(SelectedChat));
            }
        }

        private ObservableCollection<Message> _messages;
        public ObservableCollection<Message> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                OnPropertyChanged(nameof(Messages));

                if (value != null)
                    ScrollToBottom?.Invoke();
            }
        }

        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }

        public ChatViewModel(User user, Chat chat)
        {
            SelectedChat = chat;
            CurrentUser = user;

            Package package = new(Command.GetMessages, chat.Id_Chat);
            ServerContext.SendRequest(package);
            Message = new Message(CurrentUser, chat);

            PopupCommand = new RelayCommand(Popup);
            LeaveChatCommand = new RelayCommand(LeaveChat);
            SendMessageCommand = new RelayCommand(SendMessage);
            SaveFileCommand = new RelayCommand(SaveFile);
            AddFileCommand = new RelayCommand(AddFile);
            RemoveFileCommand = new RelayCommand(RemoveFile);
            CallCommand = new RelayCommand(Call);
        }

        public override void SubscribeEvent()
        {
            ServerContext.MessageCmdReceived += OnMessageCmdReceived;
            ServerContext.GetMessagesCmdReceived += OnGetMessagesCmdReceived;
            ServerContext.GetFileCmdReceived += OnGetFileCmdReceived;
        }


        public override void UnsubscribeFromEvent()
        {
            ServerContext.MessageCmdReceived -= OnMessageCmdReceived;
            ServerContext.GetMessagesCmdReceived -= OnGetMessagesCmdReceived;
            ServerContext.GetFileCmdReceived -= OnGetFileCmdReceived;
        }

        #region Обработка событий получения ответа от сервера

        private void OnGetFileCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Attachment attachment)
            {
                SaveFileDialog saveFileDialog = new();
                saveFileDialog.FileName = attachment.FileName;
                saveFileDialog.Filter = $"Attachment|*.{attachment.FileName.Split('.')[^1]}";
                if (saveFileDialog.ShowDialog() == true)
                    FileHelper.SaveFile(saveFileDialog.FileName, attachment.File);
            }
        }

        private void OnGetMessagesCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is List<Message> messages)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    Messages = new ObservableCollection<Message>(messages));
            }
        }

        /// <summary>
        /// Добавление сообщения
        /// </summary>
        /// <param name="message">Сообщение</param>
        private void OnMessageCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Message message)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if ((Messages != null) && (SelectedChat != null) && (SelectedChat.Id_Chat == message.Chat_Id))
                    {
                        Messages.Add(message);
                        if (message != null)
                            ScrollToBottom();
                    }
                });
            }
        }
        #endregion

        private void Popup(object obj) =>
            OnViewSwitched(new ChatManagementViewModel(SelectedChat, CurrentUser), ViewType.Popup);

        private void LeaveChat(object obj)
        {
            UserChatDTO userChatDTO = new(CurrentUser.Email, SelectedChat.Id_Chat);
            Package package = new(Command.RemoveUserFromChat, userChatDTO);
            ServerContext.SendRequest(package);
        }

        private void SendMessage(object obj)
        {
            if ((!string.IsNullOrEmpty(Message.Body)) && (SelectedChat != null))
            {
                Package package = new(Command.Message, Message);
                ServerContext.SendRequest(package);

                Message = new Message(CurrentUser, SelectedChat);
                OnPropertyChanged(nameof(Message.File));
                OnPropertyChanged(nameof(Message.Body));
            }
        }

        private void SaveFile(object obj)
        {
            Message message = Messages.FirstOrDefault(m => m.Id_Message == (int)obj);
            message.From = null;

            Package package = new(Command.GetFile, message);
            ServerContext.SendRequest(package);
        }

        private void AddFile(object obj)
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Image|*.jpg;*.png|Word|*.docx|Excel|*.xlsx";
            if (openFileDialog.ShowDialog() == true)
            {
                Message = FileHelper.OpenFile(Message, openFileDialog.FileName) as Message;
                OnPropertyChanged(nameof(Message.File));
            }
        }

        private void RemoveFile(object obj)
        {
            Message.File = null;
            Message.FileName = null;
            OnPropertyChanged(nameof(Message.FileName));
        }

        private void Call(object obj)
        {
            IPUser caller = new(IPHelper.GetIP(), CurrentUser.Email);
            Call call = new(SelectedChat.Users.FirstOrDefault(u => u.Email != _currentUser.Email).Email, caller);
            Package package = new(Command.Call, call);
            ServerContext.SendRequest(package);
        }
    }
}
