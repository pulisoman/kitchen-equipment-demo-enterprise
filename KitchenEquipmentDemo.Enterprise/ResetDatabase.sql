-- ===========================================
-- RESET DATABASE TO CLEAN STATE (all tables)
-- ===========================================
USE [KitchenEquipmentDemo.Enterprise];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    -- Disable FK constraints
    ALTER TABLE dbo.site_equipment_history      NOCHECK CONSTRAINT ALL;
    ALTER TABLE dbo.equipment                   NOCHECK CONSTRAINT ALL;
    ALTER TABLE dbo.site                        NOCHECK CONSTRAINT ALL;
    ALTER TABLE dbo.user_registration_request   NOCHECK CONSTRAINT ALL;
    ALTER TABLE dbo.[user]                      NOCHECK CONSTRAINT ALL;

    -- Delete (children -> parents)
    DELETE FROM dbo.site_equipment_history;
    DELETE FROM dbo.equipment;
    DELETE FROM dbo.site;
    DELETE FROM dbo.user_registration_request;
    DELETE FROM dbo.[user];

    -- Reseed identity columns
    DBCC CHECKIDENT ('dbo.site_equipment_history', RESEED, 0);
    DBCC CHECKIDENT ('dbo.equipment',              RESEED, 0);
    DBCC CHECKIDENT ('dbo.site',                   RESEED, 0);
    DBCC CHECKIDENT ('dbo.user_registration_request', RESEED, 0);
    DBCC CHECKIDENT ('dbo.[user]',                 RESEED, 0);

    -- Re-enable and validate constraints
    ALTER TABLE dbo.site_equipment_history      WITH CHECK CHECK CONSTRAINT ALL;
    ALTER TABLE dbo.equipment                   WITH CHECK CHECK CONSTRAINT ALL;
    ALTER TABLE dbo.site                        WITH CHECK CHECK CONSTRAINT ALL;
    ALTER TABLE dbo.user_registration_request   WITH CHECK CHECK CONSTRAINT ALL;
    ALTER TABLE dbo.[user]                      WITH CHECK CHECK CONSTRAINT ALL;

    COMMIT;
    PRINT 'Database reset complete. All data wiped; identities reseeded.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    THROW;
END CATCH;
GO

-- Sanity check
SELECT
  (SELECT COUNT(*) FROM dbo.[user])                       AS Users,
  (SELECT COUNT(*) FROM dbo.site)                         AS Sites,
  (SELECT COUNT(*) FROM dbo.equipment)                    AS Equipments,
  (SELECT COUNT(*) FROM dbo.site_equipment_history)       AS SiteEquipmentHistory,
  (SELECT COUNT(*) FROM dbo.user_registration_request)    AS RegistrationRequests;
