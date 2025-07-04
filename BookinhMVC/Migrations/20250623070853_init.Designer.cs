﻿// <auto-generated />
using System;
using BookinhMVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BookinhMVC.Migrations
{
    [DbContext(typeof(BookingContext))]
    [Migration("20250623070853_init")]
    partial class init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BookinhMVC.Models.BacSi", b =>
                {
                    b.Property<int>("MaBacSi")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaBacSi"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HinhAnhBacSi")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HoTen")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MaKhoa")
                        .HasColumnType("int");

                    b.Property<int>("MaNguoiDung")
                        .HasColumnType("int");

                    b.Property<string>("MoTa")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("NgayTao")
                        .HasColumnType("datetime2");

                    b.Property<string>("SoDienThoai")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MaBacSi");

                    b.HasIndex("MaKhoa");

                    b.HasIndex("MaNguoiDung")
                        .IsUnique();

                    b.ToTable("BacSis");
                });

            modelBuilder.Entity("BookinhMVC.Models.BenhNhan", b =>
                {
                    b.Property<int>("MaBenhNhan")
                        .HasColumnType("int");

                    b.Property<string>("DiaChi")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GioiTinh")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HinhAnhBenhNhan")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HoTen")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("NgaySinh")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("NgayTao")
                        .HasColumnType("datetime2");

                    b.Property<string>("SoBaoHiem")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SoDienThoai")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MaBenhNhan");

                    b.ToTable("BenhNhans");
                });

            modelBuilder.Entity("BookinhMVC.Models.ChatMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsRead")
                        .HasColumnType("bit");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ReceiverId")
                        .HasColumnType("int");

                    b.Property<int>("SenderId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("ChatMessages");
                });

            modelBuilder.Entity("BookinhMVC.Models.CsKh", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("CsKhs");
                });

            modelBuilder.Entity("BookinhMVC.Models.DanhGia", b =>
                {
                    b.Property<int>("MaDanhGia")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaDanhGia"));

                    b.Property<int>("DiemDanhGia")
                        .HasColumnType("int");

                    b.Property<int>("MaBacSi")
                        .HasColumnType("int");

                    b.Property<int>("MaBenhNhan")
                        .HasColumnType("int");

                    b.Property<DateTime>("NgayDanhGia")
                        .HasColumnType("datetime2");

                    b.Property<string>("NhanXet")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MaDanhGia");

                    b.HasIndex("MaBacSi");

                    b.HasIndex("MaBenhNhan");

                    b.ToTable("DanhGias");
                });

            modelBuilder.Entity("BookinhMVC.Models.HoSoBenhAn", b =>
                {
                    b.Property<int>("MaHoSo")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaHoSo"));

                    b.Property<string>("ChanDoan")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MaBacSi")
                        .HasColumnType("int");

                    b.Property<int>("MaBenhNhan")
                        .HasColumnType("int");

                    b.Property<DateTime>("NgayKham")
                        .HasColumnType("datetime2");

                    b.Property<string>("PhuongAnDieuTri")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MaHoSo");

                    b.HasIndex("MaBacSi");

                    b.HasIndex("MaBenhNhan");

                    b.ToTable("HoSoBenhAns");
                });

            modelBuilder.Entity("BookinhMVC.Models.Khoa", b =>
                {
                    b.Property<int>("MaKhoa")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaKhoa"));

                    b.Property<string>("MoTa")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("NgayTao")
                        .HasColumnType("datetime2");

                    b.Property<string>("TenKhoa")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MaKhoa");

                    b.ToTable("Khoas");
                });

            modelBuilder.Entity("BookinhMVC.Models.LichHen", b =>
                {
                    b.Property<int>("MaLich")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaLich"));

                    b.Property<int>("MaBacSi")
                        .HasColumnType("int");

                    b.Property<int>("MaBenhNhan")
                        .HasColumnType("int");

                    b.Property<DateTime>("NgayGio")
                        .HasColumnType("datetime2");

                    b.Property<string>("TrangThai")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TrieuChung")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MaLich");

                    b.HasIndex("MaBacSi");

                    b.HasIndex("MaBenhNhan");

                    b.ToTable("LichHens");
                });

            modelBuilder.Entity("BookinhMVC.Models.LichLamViec", b =>
                {
                    b.Property<int>("MaLich")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaLich"));

                    b.Property<TimeSpan>("GioBatDau")
                        .HasColumnType("time");

                    b.Property<TimeSpan>("GioKetThuc")
                        .HasColumnType("time");

                    b.Property<int>("MaBacSi")
                        .HasColumnType("int");

                    b.Property<DateTime>("NgayLamViec")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("NgayTao")
                        .HasColumnType("datetime2");

                    b.Property<string>("ThuTrongTuan")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TrangThai")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MaLich");

                    b.HasIndex("MaBacSi");

                    b.ToTable("LichLamViecs");
                });

            modelBuilder.Entity("BookinhMVC.Models.NguoiDung", b =>
                {
                    b.Property<int>("MaNguoiDung")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaNguoiDung"));

                    b.Property<string>("MatKhau")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("NgayTao")
                        .HasColumnType("datetime2");

                    b.Property<string>("TenDangNhap")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("VaiTro")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MaNguoiDung");

                    b.ToTable("NguoiDungs");
                });

            modelBuilder.Entity("BookinhMVC.Models.Question", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Answer")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("AnsweredAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Category")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DoctorId")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DoctorId");

                    b.HasIndex("UserId");

                    b.ToTable("Questions");
                });

            modelBuilder.Entity("BookinhMVC.Models.BacSi", b =>
                {
                    b.HasOne("BookinhMVC.Models.Khoa", "Khoa")
                        .WithMany("BacSis")
                        .HasForeignKey("MaKhoa")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BookinhMVC.Models.NguoiDung", "NguoiDung")
                        .WithOne("BacSi")
                        .HasForeignKey("BookinhMVC.Models.BacSi", "MaNguoiDung")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Khoa");

                    b.Navigation("NguoiDung");
                });

            modelBuilder.Entity("BookinhMVC.Models.BenhNhan", b =>
                {
                    b.HasOne("BookinhMVC.Models.NguoiDung", "NguoiDung")
                        .WithOne("BenhNhan")
                        .HasForeignKey("BookinhMVC.Models.BenhNhan", "MaBenhNhan")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("NguoiDung");
                });

            modelBuilder.Entity("BookinhMVC.Models.DanhGia", b =>
                {
                    b.HasOne("BookinhMVC.Models.BacSi", "BacSi")
                        .WithMany("DanhGias")
                        .HasForeignKey("MaBacSi")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("BookinhMVC.Models.BenhNhan", "BenhNhan")
                        .WithMany("DanhGias")
                        .HasForeignKey("MaBenhNhan")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("BacSi");

                    b.Navigation("BenhNhan");
                });

            modelBuilder.Entity("BookinhMVC.Models.HoSoBenhAn", b =>
                {
                    b.HasOne("BookinhMVC.Models.BacSi", "BacSi")
                        .WithMany("HoSoBenhAns")
                        .HasForeignKey("MaBacSi")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("BookinhMVC.Models.BenhNhan", "BenhNhan")
                        .WithMany("HoSoBenhAns")
                        .HasForeignKey("MaBenhNhan")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("BacSi");

                    b.Navigation("BenhNhan");
                });

            modelBuilder.Entity("BookinhMVC.Models.LichHen", b =>
                {
                    b.HasOne("BookinhMVC.Models.BacSi", "BacSi")
                        .WithMany("LichHens")
                        .HasForeignKey("MaBacSi")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("BookinhMVC.Models.BenhNhan", "BenhNhan")
                        .WithMany("LichHens")
                        .HasForeignKey("MaBenhNhan")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("BacSi");

                    b.Navigation("BenhNhan");
                });

            modelBuilder.Entity("BookinhMVC.Models.LichLamViec", b =>
                {
                    b.HasOne("BookinhMVC.Models.BacSi", "BacSi")
                        .WithMany("LichLamViecs")
                        .HasForeignKey("MaBacSi")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BacSi");
                });

            modelBuilder.Entity("BookinhMVC.Models.Question", b =>
                {
                    b.HasOne("BookinhMVC.Models.BacSi", "Doctor")
                        .WithMany()
                        .HasForeignKey("DoctorId");

                    b.HasOne("BookinhMVC.Models.NguoiDung", "User")
                        .WithMany("Questions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Doctor");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BookinhMVC.Models.BacSi", b =>
                {
                    b.Navigation("DanhGias");

                    b.Navigation("HoSoBenhAns");

                    b.Navigation("LichHens");

                    b.Navigation("LichLamViecs");
                });

            modelBuilder.Entity("BookinhMVC.Models.BenhNhan", b =>
                {
                    b.Navigation("DanhGias");

                    b.Navigation("HoSoBenhAns");

                    b.Navigation("LichHens");
                });

            modelBuilder.Entity("BookinhMVC.Models.Khoa", b =>
                {
                    b.Navigation("BacSis");
                });

            modelBuilder.Entity("BookinhMVC.Models.NguoiDung", b =>
                {
                    b.Navigation("BacSi")
                        .IsRequired();

                    b.Navigation("BenhNhan")
                        .IsRequired();

                    b.Navigation("Questions");
                });
#pragma warning restore 612, 618
        }
    }
}
