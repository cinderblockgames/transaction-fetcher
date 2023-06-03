const api = require('@actual-app/api');

async function addTransactions(connectionInfo, accountId, transactions) {
    // Connect and sync up.
    await api.init({ serverURL: connectionInfo.serverUrl,  password: connectionInfo.serverPassword });
    await api.downloadBudget(connectionInfo.budgetSyncId);

    // Add the new transactions.
    await api.addTransactions(accountId, transactions);

    // All done.
    await api.shutdown();
}

module.exports = { addTransactions };