WCell
=====

World of Warcraft emulator written in C#/.NET 4.0, with design and extensibility in mind.

### Why this branch exists
- Fluent Hibernate Re-write branch initiated by Jaddie, potentially very unstable and also may not even build!

### The tasks & goals involved in this branch
* Task: Re-write all and any file which references the database, e.g ActiveRecord files due to cleaning out that horrible old code!
* Any old SQL code, especially Sqlutil should end up obsolete upon branch completion & as already noted in AuthServer, markedly higher speeds should be possible.
* General increase in performance and also stabiity and durability of overall project, any DB file should be relocated to the new WCell database folder (relative to project).

### When this branch be merged & deleted
- This branch should be a valid candidate for a master merge when the above tasks are done and the project is stable and fully capable of running & not encountering DB related issues
- The branch will be at a stage that is qualified for deletion upon no critical level bugs or blockers are reported on http://bugs.wcell.org/
