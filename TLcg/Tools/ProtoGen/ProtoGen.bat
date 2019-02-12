call protogen -i:BlockInfo_Client.proto -o:BlockInfo.cs
xcopy /y BlockInfo.cs ..\..\Assets\Scripts\Proto\BlockInfo.cs
pause