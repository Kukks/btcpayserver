﻿// <auto-generated />
using System;
using BTCPayServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BTCPayServer.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20191221152809_RemoveOpenIddict")]
    partial class RemoveOpenIddict
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity("BTCPayServer.Data.APIKeyData", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(50);

                    b.Property<string>("StoreId")
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("ApiKeys");
                });

            modelBuilder.Entity("BTCPayServer.Data.AddressInvoiceData", b =>
                {
                    b.Property<string>("Address")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset?>("CreatedTime");

                    b.Property<string>("InvoiceDataId");

                    b.HasKey("Address");

                    b.HasIndex("InvoiceDataId");

                    b.ToTable("AddressInvoices");
                });

            modelBuilder.Entity("BTCPayServer.Data.AppData", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AppType");

                    b.Property<DateTimeOffset>("Created");

                    b.Property<string>("Name");

                    b.Property<string>("Settings");

                    b.Property<string>("StoreDataId");

                    b.Property<bool>("TagAllInvoices");

                    b.HasKey("Id");

                    b.HasIndex("StoreDataId");

                    b.ToTable("Apps");
                });

            modelBuilder.Entity("BTCPayServer.Data.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<bool>("RequiresEmailConfirmation");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("BTCPayServer.Data.HistoricalAddressInvoiceData", b =>
                {
                    b.Property<string>("InvoiceDataId");

                    b.Property<string>("Address");

                    b.Property<DateTimeOffset>("Assigned");

                    b.Property<string>("CryptoCode");

                    b.Property<DateTimeOffset?>("UnAssigned");

                    b.HasKey("InvoiceDataId", "Address");

                    b.ToTable("HistoricalAddressInvoices");
                });

            modelBuilder.Entity("BTCPayServer.Data.InvoiceData", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("Blob");

                    b.Property<DateTimeOffset>("Created");

                    b.Property<string>("CustomerEmail");

                    b.Property<string>("ExceptionStatus");

                    b.Property<string>("ItemCode");

                    b.Property<string>("OrderId");

                    b.Property<string>("Status");

                    b.Property<string>("StoreDataId");

                    b.HasKey("Id");

                    b.HasIndex("StoreDataId");

                    b.ToTable("Invoices");
                });

            modelBuilder.Entity("BTCPayServer.Data.InvoiceEventData", b =>
                {
                    b.Property<string>("InvoiceDataId");

                    b.Property<string>("UniqueId");

                    b.Property<string>("Message");

                    b.Property<DateTimeOffset>("Timestamp");

                    b.HasKey("InvoiceDataId", "UniqueId");

                    b.ToTable("InvoiceEvents");
                });

            modelBuilder.Entity("BTCPayServer.Data.PairedSINData", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Label");

                    b.Property<DateTimeOffset>("PairingTime");

                    b.Property<string>("SIN");

                    b.Property<string>("StoreDataId");

                    b.HasKey("Id");

                    b.HasIndex("SIN");

                    b.HasIndex("StoreDataId");

                    b.ToTable("PairedSINData");
                });

            modelBuilder.Entity("BTCPayServer.Data.PairingCodeData", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTimeOffset>("Expiration");

                    b.Property<string>("Facade");

                    b.Property<string>("Label");

                    b.Property<string>("SIN");

                    b.Property<string>("StoreDataId");

                    b.Property<string>("TokenValue");

                    b.HasKey("Id");

                    b.ToTable("PairingCodes");
                });

            modelBuilder.Entity("BTCPayServer.Data.PaymentData", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Accounted");

                    b.Property<byte[]>("Blob");

                    b.Property<string>("InvoiceDataId");

                    b.HasKey("Id");

                    b.HasIndex("InvoiceDataId");

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("BTCPayServer.Data.PaymentRequestData", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("Blob");

                    b.Property<DateTimeOffset>("Created")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

                    b.Property<int>("Status");

                    b.Property<string>("StoreDataId");

                    b.HasKey("Id");

                    b.HasIndex("Status");

                    b.HasIndex("StoreDataId");

                    b.ToTable("PaymentRequests");
                });

            modelBuilder.Entity("BTCPayServer.Data.PendingInvoiceData", b =>
                {
                    b.Property<string>("Id");

                    b.HasKey("Id");

                    b.ToTable("PendingInvoices");
                });

            modelBuilder.Entity("BTCPayServer.Data.RefundAddressesData", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("Blob");

                    b.Property<string>("InvoiceDataId");

                    b.HasKey("Id");

                    b.HasIndex("InvoiceDataId");

                    b.ToTable("RefundAddresses");
                });

            modelBuilder.Entity("BTCPayServer.Data.SettingData", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("BTCPayServer.Data.StoreData", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DefaultCrypto");

                    b.Property<string>("DerivationStrategies");

                    b.Property<string>("DerivationStrategy");

                    b.Property<int>("SpeedPolicy");

                    b.Property<byte[]>("StoreBlob");

                    b.Property<byte[]>("StoreCertificate");

                    b.Property<string>("StoreName");

                    b.Property<string>("StoreWebsite");

                    b.HasKey("Id");

                    b.ToTable("Stores");
                });

            modelBuilder.Entity("BTCPayServer.Data.StoredFile", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApplicationUserId");

                    b.Property<string>("FileName");

                    b.Property<string>("StorageFileName");

                    b.Property<DateTime>("Timestamp");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationUserId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("BTCPayServer.Data.U2FDevice", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApplicationUserId");

                    b.Property<byte[]>("AttestationCert")
                        .IsRequired();

                    b.Property<int>("Counter");

                    b.Property<byte[]>("KeyHandle")
                        .IsRequired();

                    b.Property<string>("Name");

                    b.Property<byte[]>("PublicKey")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("ApplicationUserId");

                    b.ToTable("U2FDevices");
                });

            modelBuilder.Entity("BTCPayServer.Data.UserStore", b =>
                {
                    b.Property<string>("ApplicationUserId");

                    b.Property<string>("StoreDataId");

                    b.Property<string>("Role");

                    b.HasKey("ApplicationUserId", "StoreDataId");

                    b.HasIndex("StoreDataId");

                    b.ToTable("UserStore");
                });

            modelBuilder.Entity("BTCPayServer.Data.WalletData", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("Blob");

                    b.HasKey("Id");

                    b.ToTable("Wallets");
                });

            modelBuilder.Entity("BTCPayServer.Data.WalletTransactionData", b =>
                {
                    b.Property<string>("WalletDataId");

                    b.Property<string>("TransactionId");

                    b.Property<byte[]>("Blob");

                    b.Property<string>("Labels");

                    b.HasKey("WalletDataId", "TransactionId");

                    b.ToTable("WalletTransactions");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("BTCPayServer.Data.APIKeyData", b =>
                {
                    b.HasOne("BTCPayServer.Data.StoreData", "StoreData")
                        .WithMany("APIKeys")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BTCPayServer.Data.AddressInvoiceData", b =>
                {
                    b.HasOne("BTCPayServer.Data.InvoiceData", "InvoiceData")
                        .WithMany("AddressInvoices")
                        .HasForeignKey("InvoiceDataId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BTCPayServer.Data.AppData", b =>
                {
                    b.HasOne("BTCPayServer.Data.StoreData", "StoreData")
                        .WithMany("Apps")
                        .HasForeignKey("StoreDataId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BTCPayServer.Data.HistoricalAddressInvoiceData", b =>
                {
                    b.HasOne("BTCPayServer.Data.InvoiceData", "InvoiceData")
                        .WithMany("HistoricalAddressInvoices")
                        .HasForeignKey("InvoiceDataId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BTCPayServer.Data.InvoiceData", b =>
                {
                    b.HasOne("BTCPayServer.Data.StoreData", "StoreData")
                        .WithMany("Invoices")
                        .HasForeignKey("StoreDataId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BTCPayServer.Data.InvoiceEventData", b =>
                {
                    b.HasOne("BTCPayServer.Data.InvoiceData", "InvoiceData")
                        .WithMany("Events")
                        .HasForeignKey("InvoiceDataId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BTCPayServer.Data.PairedSINData", b =>
                {
                    b.HasOne("BTCPayServer.Data.StoreData", "StoreData")
                        .WithMany("PairedSINs")
                        .HasForeignKey("StoreDataId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BTCPayServer.Data.PaymentData", b =>
                {
                    b.HasOne("BTCPayServer.Data.InvoiceData", "InvoiceData")
                        .WithMany("Payments")
                        .HasForeignKey("InvoiceDataId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BTCPayServer.Data.PaymentRequestData", b =>
                {
                    b.HasOne("BTCPayServer.Data.StoreData", "StoreData")
                        .WithMany("PaymentRequests")
                        .HasForeignKey("StoreDataId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BTCPayServer.Data.PendingInvoiceData", b =>
                {
                    b.HasOne("BTCPayServer.Data.InvoiceData", "InvoiceData")
                        .WithMany("PendingInvoices")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BTCPayServer.Data.RefundAddressesData", b =>
                {
                    b.HasOne("BTCPayServer.Data.InvoiceData", "InvoiceData")
                        .WithMany("RefundAddresses")
                        .HasForeignKey("InvoiceDataId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BTCPayServer.Data.StoredFile", b =>
                {
                    b.HasOne("BTCPayServer.Data.ApplicationUser", "ApplicationUser")
                        .WithMany("StoredFiles")
                        .HasForeignKey("ApplicationUserId");
                });

            modelBuilder.Entity("BTCPayServer.Data.U2FDevice", b =>
                {
                    b.HasOne("BTCPayServer.Data.ApplicationUser", "ApplicationUser")
                        .WithMany("U2FDevices")
                        .HasForeignKey("ApplicationUserId");
                });

            modelBuilder.Entity("BTCPayServer.Data.UserStore", b =>
                {
                    b.HasOne("BTCPayServer.Data.ApplicationUser", "ApplicationUser")
                        .WithMany("UserStores")
                        .HasForeignKey("ApplicationUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("BTCPayServer.Data.StoreData", "StoreData")
                        .WithMany("UserStores")
                        .HasForeignKey("StoreDataId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BTCPayServer.Data.WalletTransactionData", b =>
                {
                    b.HasOne("BTCPayServer.Data.WalletData", "WalletData")
                        .WithMany("WalletTransactions")
                        .HasForeignKey("WalletDataId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("BTCPayServer.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("BTCPayServer.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("BTCPayServer.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("BTCPayServer.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
