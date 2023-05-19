# cms.idseefeld

My personal website initally created for Umbraco Cloud now hosted at IONOS.

Development environment uses *user secrets* for SMTP password and is omitted in appsettings.Development.json. In terminal window for Cms.Idseefeld.Web project execute:
```bash
- dotnet user-secrets init
- dotnet user-secrets set "Umbraco:CMS:Global:Smtp:Password" "[YOUR PASSWORD]"
```

