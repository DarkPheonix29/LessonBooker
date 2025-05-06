using Google.Cloud.Firestore;
using System;

[FirestoreData]
public class KeyData
{
	[FirestoreDocumentId] // Maps to the Firestore document ID
	public string Id { get; set; }

	[FirestoreProperty("key")]
	public string Key { get; set; }

	[FirestoreProperty("used")]
	public bool Used { get; set; }

	[FirestoreProperty("createdAt")]
	public Timestamp CreatedAt { get; set; }
}
