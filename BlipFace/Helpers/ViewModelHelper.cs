﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlipFace.Service.Entities;
using BlipFace.Model;
using System.Collections.ObjectModel;

namespace BlipFace.Helpers
{
    /// <summary>
    /// Klasa pomocnicza
    /// </summary>
    public class ViewModelHelper
    {
        /// <summary>
        /// Pomocna metoda do mapowania Entities do ViewEntities
        /// </summary>
        /// <param name="statusesList"></param>
        /// <returns></returns>
        public static IList<StatusViewModel> MapToViewStatus(IList<BlipStatus> statusesList, string ownerLogin)
        {
            IList<StatusViewModel> statuses =
                new  ObservableCollection<StatusViewModel>();


            //IList<StatusViewModel> sts = new List<StatusViewModel>(statusesList.Count);
            try
            {
                foreach (BlipStatus status in statusesList)
                {
                    //todo: trzeba uważać bo gdy nie ma recipient to 
                    //rzuca wyjątekiem nullreference
                    string reciptientAvatar = string.Empty;
                    string reciptientLogin = string.Empty;
                    bool hasRecipient = false;
                    bool directedMessage = false;
                    bool privateMessage = false;

                    if (status.Type == "DirectedMessage")
                    {
                        reciptientAvatar = status.Recipient.Avatar == null
                                           ? "http://static1.blip.pl/images/nn_nano.png?1240395130"
                                           : status.Recipient.Avatar.Url50;
                        reciptientLogin = status.Recipient.Login;
                        hasRecipient = true;
                        directedMessage = true;
                    }
                    else if (status.Type == "PrivateMessage")
                    {
                        reciptientAvatar = status.Recipient.Avatar == null
                                           ? "http://static1.blip.pl/images/nn_nano.png?1240395130"
                                           : status.Recipient.Avatar.Url50;
                        reciptientLogin = status.Recipient.Login;
                        hasRecipient = true;
                        privateMessage = true;
                    }

                    //czasami data nie jest ustawiana przez Blipa - dziwne
                    string creationDate = status.StatusTime == null ? string.Empty : status.StatusTime;
                    string avatarUrl = status.User.Avatar == null
                                           ? "http://static1.blip.pl/images/nn_nano.png?1240395130"
                                           : status.User.Avatar.Url50;
                    string pictureUrl = status.Pictures == null ? string.Empty : status.Pictures[0].Url;

                    statuses.Add(new StatusViewModel()
                                {
                                    StatusId = status.Id,
                                    UserId = status.User.Id,
                                    Content = status.Content,
                                    UserAvatar50 = avatarUrl,
                                    RecipientAvatar50 = reciptientAvatar,
                                    RecipientLogin = reciptientLogin,
                                    HasRecipient = hasRecipient,
                                    DirectedMessage = directedMessage,
                                    PrivateMessage = privateMessage,
                                    CreationDate = creationDate,
                                    UserLogin = status.User.Login,
                                    Type = status.Type,
                                    BlipFaceUser = ownerLogin,
                                    FirstPictureUrl= pictureUrl
                                });
                }
            }
            catch (Exception e)
            {
                throw;
            }


            return statuses;
        }

        /// <summary>
        /// Pomocna metoda do mapowania Entities do ViewEntities
        /// </summary>
        /// <param name="iList"></param>
        /// <returns></returns>
        public static StatusViewModel MapToViewStatus(BlipFace.Service.Entities.BlipStatus status)
        {
            StatusViewModel st = new StatusViewModel();
            try
            {
                //todo: trzeba uważać bo gdy nie ma recipient to 
                //rzuca wyjątekiem nullreference
                string reciptientAvatar = string.Empty;
                string reciptientLogin = string.Empty;

                //czasami data nie jest ustawiana przez Blipa - dziwne
                string creationDate = string.Empty;
                string avatarUrl = status.User.Avatar == null
                                       ? "http://static1.blip.pl/images/nn_nano.png?1240395130"
                                       : status.User.Avatar.Url50;

                st.StatusId = status.Id;
                st.UserId = status.User.Id;
                st.Content = status.Content;
                st.UserAvatar50 = avatarUrl;
                st.RecipientAvatar50 = reciptientAvatar;
                st.RecipientLogin = reciptientLogin;
                st.CreationDate = creationDate;
                st.UserLogin = status.User.Login;
            }
            catch (Exception e)
            {
                throw;
            }


            return st;
        }
    }
}