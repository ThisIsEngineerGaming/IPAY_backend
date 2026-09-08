# IPAY_backend

A .NET Clean Architecture backend (Domain / Application / Infastructure / WebApi) backed by
Firebase — Cloud Firestore for data, Cloud Storage for images.

## Running this project on a new machine

The Firebase service-account key is **not** included in this repository (it grants full
admin access to the Firestore database and Storage bucket, bypassing all security rules — it
must never be committed to git). To run the project locally you need to supply your own:

1. In the [Firebase console](https://console.firebase.google.com), open the project →
   ⚙️ Project settings → **Service accounts** tab → **Generate new private key**. This
   downloads a JSON file.
2. Rename it to `firebase-service-account.json` and place it in
   `src/ExamTest.WebApi/` (next to `Program.cs`). See
   `firebase-service-account.example.json` in that same folder for the expected shape.
3. Check `appsettings.json`'s `Firebase` section — `ProjectId` and `StorageBucket` should
   match your own Firebase project if you're not using the original one.
4. Build and run as usual. `.gitignore` already excludes the real key file, so it's safe to
   have it sitting in that folder — it won't accidentally get committed.

If you're picking up *this* project specifically (same Firebase project, not a fresh one),
ask the project owner to send you the key file directly through a private channel (not git,
not a public share link) — Discord DM, email, USB stick, whatever. There's no way around
that: the app needs the real credential to talk to that Firebase project, and there's no
substitute for actually having it.

### A note on key hygiene

If this key is ever exposed publicly (e.g. accidentally pushed before `.gitignore` caught
it), don't try to "fix" it by rotating or re-encoding it in place — go to the Firebase
console → Service accounts and revoke/delete that key, then generate a fresh one. Also worth
checking, in Google Cloud Console → IAM, that the service account only has the roles it
actually needs (Firestore + Storage), not a broad `Editor`/`Owner` role — that limits the
blast radius if a key ever does leak.
