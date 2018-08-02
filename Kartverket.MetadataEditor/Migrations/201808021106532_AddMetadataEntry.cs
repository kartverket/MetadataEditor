namespace Kartverket.MetadataEditor.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMetadataEntry : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MetaDataEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Uuid = c.String(),
                        Title = c.String(),
                        OrganizationName = c.String(),
                        ContactEmail = c.String(),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Errors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Key = c.String(),
                        Message = c.String(),
                        MetaDataEntry_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MetaDataEntries", t => t.MetaDataEntry_Id)
                .Index(t => t.MetaDataEntry_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Errors", "MetaDataEntry_Id", "dbo.MetaDataEntries");
            DropIndex("dbo.Errors", new[] { "MetaDataEntry_Id" });
            DropTable("dbo.Errors");
            DropTable("dbo.MetaDataEntries");
        }
    }
}
