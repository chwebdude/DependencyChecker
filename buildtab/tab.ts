import Controls = require("VSS/Controls");
import VSS_Service = require("VSS/Service");
import TFS_Build_Contracts = require("TFS/Build/Contracts");
import TFS_Build_Extension_Contracts = require("TFS/Build/ExtensionContracts");
import DT_Client = require("TFS/DistributedTask/TaskRestClient");
import Mustache = require("mustache");

export class InfoTab extends Controls.BaseControl {
	constructor() {
		super();
	}

	public initialize(): void {
		super.initialize();
		// Get configuration that's shared between extension and the extension host
		var sharedConfig: TFS_Build_Extension_Contracts.IBuildResultsViewExtensionConfig = VSS.getConfiguration();
		var vsoContext = VSS.getWebContext();
		console.log("vsoContext", vsoContext);
		if (sharedConfig) {
			// register your extension with host through callback
			sharedConfig.onBuildChanged((build: TFS_Build_Contracts.Build) => {
				this._initBuildInfo(build);

				var taskClient = DT_Client.getClient();
				taskClient.getPlanAttachments(vsoContext.project.id, "build", build.orchestrationPlan.planId, "dependcies_check_result").then((taskAttachments) => {

					// Get attachment
					if (taskAttachments.length == 1) {
						var recId = taskAttachments[0].recordId;
						var timelineId = taskAttachments[0].timelineId;

						// Load it
						taskClient.getAttachmentContent(vsoContext.project.id, "build", build.orchestrationPlan.planId, timelineId, recId, "dependcies_check_result", "dependcies_check_result").then((attachementContent) => {
							// It needs to deserialized -.-
							function arrayBufferToString(buffer) {
								var arr = new Uint8Array(buffer);
								var str = String.fromCharCode.apply(String, arr);
								if (/[\u0080-\uffff]/.test(str)) {
									throw new Error("this string seems to contain (still encoded) multibytes");
								}
								return str;
							}

							var summaryPageData = arrayBufferToString(attachementContent);

							//Deserialize data
							var ob = JSON.parse(summaryPageData);
							var template = $("#template").html();
							Mustache.parse(template);
							var rendered = Mustache.render(template, ob);
							$("#target").html(rendered);

							$("#notLoaded").detach();
						});
					}
				});

			});
		}
	}

	private _initBuildInfo(build: TFS_Build_Contracts.Build) {

	}
}

InfoTab.enhance(InfoTab, $(".build-info"), {});

// Notify the parent frame that the host has been loaded
VSS.notifyLoadSucceeded();

