﻿using System;
using System.IO;
using System.Security.Cryptography;

namespace shaco.ExcelDataReader.Core.OfficeCrypto
{
    internal class StandardEncryptedPackageStream : Stream
    {
        public StandardEncryptedPackageStream(Stream underlyingStream, byte[] secretKey, StandardEncryption encryption)
        {
            Cipher = CryptoHelpers.CreateCipher(encryption.CipherAlgorithm, encryption.KeySize, encryption.BlockSize, CipherMode.ECB);
            Decryptor = Cipher.CreateDecryptor(secretKey, encryption.SaltValue);

            var header = new byte[8];
            underlyingStream.Read(header, 0, 8);
            DecryptedLength = BitConverter.ToInt32(header, 0);

            // Wrap CryptoStream to override the length and dispose the cipher and transform 
            // Zip readers scan backwards from the end for the central zip directory, and could fail if its too far away
            // CryptoStream is forward-only, so assume the zip readers read everything to memory
            BaseStream = new CryptoStream(underlyingStream, Decryptor, CryptoStreamMode.Read);
        }

        public override bool CanRead {get{return BaseStream.CanRead; }}

        public override bool CanSeek {get{return BaseStream.CanSeek; }}

        public override bool CanWrite {get{return BaseStream.CanWrite; }}

        public override long Length {get{return DecryptedLength; }}

        public override long Position
        {
            get {return BaseStream.Position; }
            set {BaseStream.Position = value; }
        }

        private Stream BaseStream { get; set; }

        private SymmetricAlgorithm Cipher { get; set; }

        private ICryptoTransform Decryptor { get; set; }

        private long DecryptedLength { get; }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return BaseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (null != Decryptor) Decryptor.Dispose();
                Decryptor = null;

                if (null != ((IDisposable)Cipher)) ((IDisposable)Cipher).Dispose();
                Cipher = null;

                if (null != BaseStream) BaseStream.Dispose();
                BaseStream = null;
            }

            base.Dispose(disposing);
        }
    }
}