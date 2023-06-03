using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;

namespace TransactionFetcher.MailWrapper;

public class Imap
{
    private ConnectionInfo ConnectionInfo { get; }

    public Imap(ConnectionInfo connectionInfo)
    {
        ConnectionInfo = connectionInfo;
    }

    public async Task RunAgainstServer(Func<IImapClient, Task> action)
    {
        using var imap = new ImapClient();
        await imap.ConnectAsync(
            ConnectionInfo.Server,
            ConnectionInfo.ImapPort,
            ConnectionInfo.UseTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
        await imap.AuthenticateAsync(ConnectionInfo.Username, ConnectionInfo.Password);
        
        try
        {
            await action(imap);
        }
        finally
        {
            await imap.DisconnectAsync(true);
        }
    }

    public async Task RunAgainstFolder(Func<IMailFolder, Task> action)
    {
        await RunAgainstServer(async imap =>
        {
            var folder = ConnectionInfo.Folder != null
                ? await imap.GetFolderAsync(ConnectionInfo.Folder)
                : imap.Inbox;
            await folder.OpenAsync(FolderAccess.ReadWrite);
            await action(folder);
        });
    }
}