﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleTcp
{
    /// <summary>
    /// SimpleTcp client settings.
    /// </summary>
    public class SimpleTcpClientSettings
    {
        #region Public-Members

        /// <summary>
        /// Buffer size to use while interacting with streams. 
        /// </summary>
        public int StreamBufferSize
        {
            get
            {
                return _StreamBufferSize;
            }
            set
            {
                if (value < 1) throw new ArgumentException("StreamBufferSize must be one or greater.");
                if (value > 65536) throw new ArgumentException("StreamBufferSize must be less than 65,536.");
                _StreamBufferSize = value;
            }
        }

        /// <summary>
        /// The number of seconds to wait when attempting to connect.
        /// </summary>
        public int ConnectTimeoutSeconds
        {
            get
            {
                return _ConnectTimeoutSeconds;
            }
            set
            {
                if (value < 1) throw new ArgumentException("ConnectTimeoutSeconds must be greater than zero.");
                _ConnectTimeoutSeconds = value;
            }
        }

        /// <summary>
        /// Enable or disable acceptance of invalid SSL certificates.
        /// </summary>
        public bool AcceptInvalidCertificates = true;

        /// <summary>
        /// Enable or disable mutual authentication of SSL client and server.
        /// </summary>
        public bool MutuallyAuthenticate = true;

        #endregion

        #region Private-Members

        private int _StreamBufferSize = 65536;
        private int _ConnectTimeoutSeconds = 5;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public SimpleTcpClientSettings()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
