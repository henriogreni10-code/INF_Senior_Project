-- Supabase SQL Setup Script for INF_SP Event Management System
-- Run this script in your Supabase SQL Editor to create all required tables

-- Enable UUID extension (optional, for future use)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Drop existing tables if they exist (be careful in production!)
-- Uncomment these lines if you need to reset the database
-- DROP TABLE IF EXISTS messages CASCADE;
-- DROP TABLE IF EXISTS bookings CASCADE;
-- DROP TABLE IF EXISTS vendors CASCADE;
-- DROP TABLE IF EXISTS events CASCADE;
-- DROP TABLE IF EXISTS users CASCADE;

-- Create Users table
CREATE TABLE IF NOT EXISTS users (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "Password" VARCHAR(255) NOT NULL,
    "UserType" VARCHAR(50) NOT NULL, -- 'Organizer', 'Vendor', 'Attendee'
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create Events table
CREATE TABLE IF NOT EXISTS events (
    "Id" SERIAL PRIMARY KEY,
    "OrganizerId" INTEGER NOT NULL REFERENCES users("Id") ON DELETE RESTRICT,
    "Title" VARCHAR(200) NOT NULL,
    "Description" TEXT NOT NULL,
    "EventDate" DATE NOT NULL,
    "StartTime" TIME NOT NULL,
    "EndTime" TIME NOT NULL,
    "Location" VARCHAR(300) NOT NULL,
    "Capacity" INTEGER NOT NULL CHECK ("Capacity" >= 1 AND "Capacity" <= 10000),
    "Category" VARCHAR(100) DEFAULT '',
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create Bookings table
CREATE TABLE IF NOT EXISTS bookings (
    "Id" SERIAL PRIMARY KEY,
    "EventId" INTEGER NOT NULL REFERENCES events("Id") ON DELETE CASCADE,
    "AttendeeId" INTEGER NOT NULL REFERENCES users("Id") ON DELETE RESTRICT,
    "BookingDate" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "Status" VARCHAR(50) DEFAULT 'Confirmed',
    UNIQUE("EventId", "AttendeeId")
);

-- Create Vendors table
CREATE TABLE IF NOT EXISTS vendors (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL REFERENCES users("Id") ON DELETE CASCADE,
    "BusinessName" VARCHAR(200) NOT NULL,
    "ServiceType" VARCHAR(100) NOT NULL,
    "Description" TEXT NOT NULL,
    "PriceRange" VARCHAR(100) DEFAULT '',
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE("UserId")
);

-- Create Messages table
CREATE TABLE IF NOT EXISTS messages (
    "Id" SERIAL PRIMARY KEY,
    "SenderId" INTEGER NOT NULL REFERENCES users("Id") ON DELETE RESTRICT,
    "RecipientId" INTEGER NOT NULL REFERENCES users("Id") ON DELETE RESTRICT,
    "VendorId" INTEGER REFERENCES vendors("Id") ON DELETE SET NULL,
    "EventId" INTEGER REFERENCES events("Id") ON DELETE SET NULL,
    "MessageText" TEXT NOT NULL,
    "SentAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "IsRead" BOOLEAN DEFAULT FALSE
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_events_organizer ON events("OrganizerId");
CREATE INDEX IF NOT EXISTS idx_events_date ON events("EventDate");
CREATE INDEX IF NOT EXISTS idx_bookings_event ON bookings("EventId");
CREATE INDEX IF NOT EXISTS idx_bookings_attendee ON bookings("AttendeeId");
CREATE INDEX IF NOT EXISTS idx_vendors_user ON vendors("UserId");
CREATE INDEX IF NOT EXISTS idx_messages_sender ON messages("SenderId");
CREATE INDEX IF NOT EXISTS idx_messages_recipient ON messages("RecipientId");
CREATE INDEX IF NOT EXISTS idx_messages_vendor ON messages("VendorId");

-- Insert a test user (optional - for testing connection)
-- Password is 'test123' - you should hash passwords in production!
INSERT INTO users ("Name", "Email", "Password", "UserType")
VALUES ('Test User', 'test@example.com', 'test123', 'Organizer')
ON CONFLICT ("Email") DO NOTHING;

-- Verify the setup by counting tables
SELECT 'Setup complete!' as status, 
       (SELECT COUNT(*) FROM users) as users_count,
       (SELECT COUNT(*) FROM events) as events_count,
       (SELECT COUNT(*) FROM bookings) as bookings_count,
       (SELECT COUNT(*) FROM vendors) as vendors_count,
       (SELECT COUNT(*) FROM messages) as messages_count;
