# IONOS Windows Hosting

Es gibt eine Besonderheit bei IONOS: 

WebDAV ist standarmäßig aktiviert, sodass Sie Ihre Dateien direkt über den Windows Explorer oder andere WebDAV-Clients verwalten können. Dies erleichtert den Zugriff auf Ihre Dateien und ermöglicht eine nahtlose Integration in Ihre Arbeitsumgebung.

Allerdings blockiert WebDAV in der IONOS-Umgebung standardmäßig die Nutzung von PUT- und DELETE-Methoden. Dies kann zu Problemen führen, wenn Sie versuchen, Dateien hochzuladen oder zu löschen. Um diese Einschränkungen zu umgehen, können Sie die folgenden Schritte ausführen:

In der `web.config` lässt sich WebDAV abschalten. Zwei Einträge unter `handlers` und `modules` sind notwendig:

```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<location path="." inheritInChildApplications="false">
		<system.webServer>
			<httpProtocol>
				<customHeaders>
					<remove name="X-Powered-By" />
				</customHeaders>
			</httpProtocol>
			<security>
				<requestFiltering removeServerHeader="true" />
			</security>
			<handlers>
				<!-- Entfernt den WebDAV-Handler. Notwendig für IONOS Windows Hosting!-->
				<remove name="WebDAV" />
				
				<add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
			</handlers>
			<modules>
				<!-- Entfernt das WebDAV-Modul, das PUT/DELETE blockiert. Notwendig für IONOS Windows Hosting!-->
				<remove name="WebDAVModule" />
			</modules>
			<aspNetCore processPath="dotnet" arguments=".\Cms.Idseefeld.Web.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" />
		</system.webServer>
	</location>
</configuration>
```