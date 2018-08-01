namespace Kartverket.MetadataEditor.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OpenMetadataEndpoint : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OpenMetadataEndpoints",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Url = c.String(),
                        OrganizationName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.OpenMetadataEndpoints");
        }
    }
}
