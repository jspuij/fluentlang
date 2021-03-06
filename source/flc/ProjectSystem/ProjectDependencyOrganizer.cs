﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static FluentLang.flc.ProjectSystem.Reference;

namespace FluentLang.flc.ProjectSystem
{
	public static class ProjectDependencyOrganizer
	{
		public static IEnumerable<ProjectInfo> CreateBuildOrder(IEnumerable<ProjectInfo> projects)
		{
			// Use Kahn's Algorithm for topological sorting.
			//
			// This implementation is extremely inneficient, 
			// but completes in under a second on my machine 
			// for up to a thousand projects with the maximum number of references each 
			// and worse case ordering.
			//
			// Given that this only runs once per build,
			// this is perfectly sufficient

			var projectInfos = projects.Select(x => new KahnProjectInfo(x)).ToList();

			while(projectInfos.Count > 0)
			{
				yield return Iterate();
			}

			ProjectInfo Iterate() 
			{
				var next = projectInfos.FirstOrDefault(x => x.RemainingDependencies.Count == 0);
				if (next == null)
					throw new FlcException("Projects contain circular dependencies");
				projectInfos.Remove(next);
				foreach(var project in projectInfos)
				{
					project.RemainingDependencies.Remove(next.ProjectInfo.Name);
				}
				return next.ProjectInfo;
			}
		}

		private class KahnProjectInfo
		{
			public KahnProjectInfo(ProjectInfo projectInfo)
			{
				ProjectInfo = projectInfo;
				RemainingDependencies =
					projectInfo
					.References
					.Where(x => x.Type == ReferenceType.Project)
					.Select(x => x.Name!)
					.ToList();
			}

			public ProjectInfo ProjectInfo { get; }
			public List<string> RemainingDependencies { get; }
		}

		public static IEnumerable<ProjectInfo> GetTransitiveDependencies(IEnumerable<ProjectInfo> projects, string projectName)
		{
			var projectsDic = projects.ToDictionary(x => x.Name);
			if (!projectsDic.TryGetValue(projectName, out var target))
				throw new FlcException($"could not find project with name: {projectName}");

			var dependencies = new Dictionary<string, ProjectInfo>();

			AddTransitiveDependencies(target);

			return dependencies.Values;

			void AddTransitiveDependencies(ProjectInfo projectInfo)
			{
				dependencies.Add(projectInfo.Name, projectInfo);
				foreach (var reference in projectInfo.References.Where(x => x.Type == ReferenceType.Project))
				{
					if (dependencies.ContainsKey(reference.Name))
						continue;
					if (!projectsDic.TryGetValue(reference.Name, out var dependency))
						throw new FlcException($"could not find project with name: {reference.Name}");
					AddTransitiveDependencies(dependency);
				}
			}
		}
	}
}
