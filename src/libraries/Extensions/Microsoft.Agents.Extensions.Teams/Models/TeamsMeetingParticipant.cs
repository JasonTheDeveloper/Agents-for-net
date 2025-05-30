﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Agents.Core.Models;

namespace Microsoft.Agents.Extensions.Teams.Models
{
    /// <summary>
    /// Teams meeting participant information, detailing user Azure Active Directory and meeting participant details.
    /// </summary>
    public class TeamsMeetingParticipant
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMeetingParticipant"/> class.
        /// </summary>
        public TeamsMeetingParticipant()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMeetingParticipant"/> class.
        /// </summary>
        /// <param name="user">Teams Channel Account information for this meeting participant.</param>
        /// <param name="conversation">Conversation Account for the meeting.</param>
        /// <param name="meeting">Information specific to this participant in the specific meeting.</param>
        public TeamsMeetingParticipant(TeamsChannelAccount user, ConversationAccount conversation = null, MeetingParticipantInfo meeting = null)
        {
            User = user;
            Meeting = meeting;
            Conversation = conversation;
        }

        /// <summary>
        /// Gets or sets the participant's user information.
        /// </summary>
        /// <value>
        /// The participant's user information.
        /// </value>
        public TeamsChannelAccount User { get; set; }

        /// <summary>
        /// Gets or sets the participant's meeting information.
        /// </summary>
        /// <value>
        /// The participant's role in the meeting.
        /// </value>
        public MeetingParticipantInfo Meeting { get; set; }

        /// <summary>
        /// Gets or sets the Conversation Account for the meeting.
        /// </summary>
        /// <value>
        /// The Conversation Account for the meeting.
        /// </value>
        public ConversationAccount Conversation { get; set; }
    }
}
