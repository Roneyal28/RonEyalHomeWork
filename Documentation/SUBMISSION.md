# Submit Task 1 with GitHub Desktop

The assignment says:

- Branch: **task-01/arsenal-armory**
- Deadline: **17 September 2026**
- Send your repository link to **adigaash@gmail.com**

This folder is already a local Git repository on the required branch. A GitHub remote has not been configured or published.

## Easiest way

1. Open **GitHub Desktop** and sign in.
2. Choose **File > Add Local Repository**, then select this project folder (`UnityMobile2026Summer`).
3. Check that **Current Branch** is `task-01/arsenal-armory`.
4. After adding your images, review the changed files. Write a summary such as `Complete Task 1 Arsenal Armory` and click **Commit to task-01/arsenal-armory**.
5. Click **Publish repository**. You can name it `UnityMobile2026Summer`. If your class requires the name `TiltanMobileSummer26`, use that name. Choose visibility: a private repository requires inviting your teacher's GitHub account; a public one can be opened from its link.
6. Click **Publish repository** in the dialog. If **Publish branch** or **Push origin** appears afterward, click it too.
7. Open the repository on GitHub and select `task-01/arsenal-armory` in the branch dropdown. Confirm you can see **Assets**, **Packages**, **ProjectSettings**, and **README.md**.
8. Copy that branch page's URL and email it to your teacher before the deadline.

If you already have a class repository, use that repository instead of publishing a second one. Clone it to a separate folder, create the required branch there, copy this project's Assets, Packages, ProjectSettings, README, Documentation and .gitignore into that branch, then commit and push. Do not copy the `.git` folder or overwrite existing class work without checking it.

## Email you can copy

Subject: Task 1 - Arsenal Armory - [Your name]

Hi Adi,

Here is my Task 1 project:
[Paste your GitHub branch link]

Branch: task-01/arsenal-armory
Unity: 6000.5.3f1
Scene: Assets/Scenes/ArsenalArmory.unity

Controls: mouse to aim, left-click to fire, hold for the automatic weapon, and click the weapon buttons to switch.

Thanks,
[Your name]

## Terminal alternative

Create an **empty** GitHub repository first (do not initialize it with a README). Replace YOUR-USERNAME and YOUR-REPO below with your values.

```bash
cd /Users/roneyal/Documents/ChatGPT/UnityMobile2026Summer
git branch --show-current
git add Assets Packages ProjectSettings Documentation README.md .gitignore
git commit -m "Complete Task 1 Arsenal Armory"
git remote add origin https://github.com/YOUR-USERNAME/YOUR-REPO.git
git push -u origin task-01/arsenal-armory
```

For later changes, run `git add`, `git commit`, then `git push` again. If the files are already committed, skip the commit command. If `origin` already exists, inspect `git remote -v` rather than adding it again.

Commit `.meta` files with their assets; they preserve Unity references. The supplied `.gitignore` excludes generated caches and build files such as Library, Temp, Logs, and APKs. The Task 1 PDF requests the repository link, not an email attachment of the entire project.

GitHub's official guides: [add a local repository](https://docs.github.com/en/desktop/adding-and-cloning-repositories/adding-a-repository-from-your-local-computer-to-github-desktop) and [publish an existing project](https://docs.github.com/en/desktop/adding-and-cloning-repositories/adding-an-existing-project-to-github-using-github-desktop).
