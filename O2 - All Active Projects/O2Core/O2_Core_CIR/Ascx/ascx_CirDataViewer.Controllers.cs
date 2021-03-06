// This file is part of the OWASP O2 Platform (http://www.owasp.org/index.php/OWASP_O2_Platform) and is released under the Apache 2.0 License (http://www.apache.org/licenses/LICENSE-2.0)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using O2.Core.CIR.CirObjects;
using O2.Core.CIR.CirUtils;
using O2.DotNetWrappers.DotNet;
using O2.DotNetWrappers.ExtensionMethods;
using O2.DotNetWrappers.Filters;
using O2.DotNetWrappers.Windows;
using O2.ImportExport.OunceLabs.Ozasmt_OunceV6;
using O2.Interfaces.CIR;
using O2.Interfaces.Messages;
using O2.Kernel;
using O2.Kernel.InterfacesBaseImpl;
using O2.Views.ASCX.DataViewers;
using O2.Views.ASCX.O2Findings;
using System.Reflection;
using O2.Kernel.CodeUtils;
using O2.Core.CIR.CirCreator.DotNet;

namespace O2.Core.CIR.Ascx
{
    public partial class ascx_CirDataViewer
    {
        public ICirDataAnalysis cirDataAnalysis { get; set; }
        public TreeNode previousMouseOverNode;
        public bool runOnLoad = true;

        public ICirFunction currentSelectedFunction;

        private void onLoad()
        {
            if (runOnLoad && false == DesignMode)
            {
                PublicDI.o2MessageQueue.onMessages += o2MessageQueue_onMessages;
                cirDataAnalysis = new CirDataAnalysis();                                                
                runOnLoad = false;                                
            }
        }

        private void setBreakPointFeatures(bool isDebuggerAvailable)
        {
            addBreakpointOnSelectedMethodifBreakpointIsEnabledToolStripMenuItem.Visible = isDebuggerAvailable;
        }

        

        public void loadCirDataAnalysisObject(ICirDataAnalysis _cirDataAnalysis)
        {
            _cirDataAnalysis.onlyShowExternalFunctionsThatAreInvokedFromCFG = cirDataAnalysis.onlyShowExternalFunctionsThatAreInvokedFromCFG;
            _cirDataAnalysis.onlyShowFunctionsOrClassesWithControlFlowGraphs = cirDataAnalysis.onlyShowFunctionsOrClassesWithControlFlowGraphs;
            _cirDataAnalysis.onlyShowFunctionsWithCallersOrCallees = cirDataAnalysis.onlyShowFunctionsWithCallersOrCallees;
            cirDataAnalysis = _cirDataAnalysis;
        }

        void o2MessageQueue_onMessages(IO2Message o2Message)
        {
            if (o2Message is IM_CirAction)
            {
                var cirAction = (IM_CirAction)o2Message;
                switch (cirAction.CirAction)
                {
                    case IM_CirActions.setCirDataAnalysis:
                        loadCirDataAnalysisObject(cirAction.CirDataAnalysis);
                        updateCirDataStats();
                        break;
                    case IM_CirActions.setCirData:  // if there was a CirData file set, then add it into CirDataAnalysis
                        CirDataAnalysisUtils.addO2CirDataFile(cirDataAnalysis, cirAction.CirData);
                        updateCirDataStats();
                        break;
                }
            }
        }

        public void updateCirDataStats()
        {
            updateCirDataStats(true);
        }
        public void updateCirDataStats(bool refreshLoadedFunctions)
        {
            this.invokeOnThread(
                () =>
                {

                    laNumberOfClasses.Text = cirDataAnalysis.dCirClass_bySignature.Count.ToString();
                    laNumberOfFunctions.Text = cirDataAnalysis.dCirFunction_bySignature.Count.ToString();
                    //showLoadedClasses(); // default to Class view
                    if (refreshLoadedFunctions)
                        showLoadedFunctions(); // default to Function view

                });
        }

        public ICirDataAnalysis getCirDataAnalysisObject()
        {
            return cirDataAnalysis;
        }


        public void showSignatures(FilteredSignature filteredSignature)
        { 
        }

        public void showSignatures(List<FilteredSignature> filteredSignatures)
        {
            var signaturesToShow = new List<String>();            
            // get them all in one collection
            var filteredSignaturesAsStrings = new List<String>();
            foreach (var filteredSignature in filteredSignatures)
                filteredSignaturesAsStrings.Add(filteredSignature.sOriginalSignature);

            // now search for them on all loaded CirFunctions
            foreach (var cirFunction in cirDataAnalysis.dCirFunction_bySignature.Values)
            {
                var cirFunctionFilteredSignature = new FilteredSignature(cirFunction);
                if (filteredSignaturesAsStrings.Contains(cirFunctionFilteredSignature.sOriginalSignature))
                {
                    signaturesToShow.Add(cirFunction.FunctionSignature);
                    filteredSignaturesAsStrings.Remove(cirFunctionFilteredSignature.sOriginalSignature);
                }
                if (filteredSignaturesAsStrings.Contains(cirFunctionFilteredSignature.sSignature))
                {
                    signaturesToShow.Add(cirFunction.FunctionSignature);
                    filteredSignaturesAsStrings.Remove(cirFunctionFilteredSignature.sSignature);
                }
            }
            
          /*  foreach (var filteredSignature in filteredSignatures)
            {
                if (cirDataAnalysis.dCirFunction_bySignature.ContainsKey(filteredSignature.sOriginalSignature))
                    signaturesToShow.Add(filteredSignature.sOriginalSignature);
                else

                    DI.log.error("dropped signature is currently not loaded: {0}", filteredSignature);
            }*/
            if (filteredSignaturesAsStrings.Count > 0)
            {
                DI.log.error("There were {0} dropped signatures that could not be resolved", filteredSignaturesAsStrings.Count);
                foreach (var signatureNotFound in filteredSignaturesAsStrings)
                    DI.log.error("   {0}", signatureNotFound);
            }

            if (signaturesToShow.Count == 0)
                DI.log.error("None of the dropped signature was resolved, you need to load their CIR");
            else
                showSignatures(signaturesToShow);
        }

        public void showSignatures(List<string> signaturesToShow)
        {
            functionsViewer.showSignatures(signaturesToShow);            
        }

        public void showLoadedClasses()
        {
            this.invokeOnThread(
                () =>
                {
                    functionsViewer.Text = "Loaded Cir Classes";
                    functionsViewer.NamespaceDepthValue = 1;
                    functionsViewer.showSignatures(cirDataAnalysis.CirClasses<string>());
                });
        }

        public void showLoadedFunctions()
        {
            this.invokeOnThread(
                () =>
                {
                /*    var signaturesToShow = from ICirFunction cirFunction
                                          in cirDataAnalysis.CirFunctions<ICirFunction>() 
                                          where cirFunction.HasControlFlowGraph
                                          select cirFunction.FunctionSignature;*/
                    functionsViewer.Text = String.Format("Loaded Cir Functions");
                    functionsViewer.showSignatures(cirDataAnalysis.CirFunctions<string>());
                });
        }

        public void loadFile(string fileToLoad)
        {
            loadFile(fileToLoad, true, false);
        }

        public void loadFile(string sFileToLoad, bool remapXrefs, bool decompileCodeIfNoPdb)
        {
            DI.log.info("in CirDataViewer->LoadingFile :{0}", sFileToLoad);
            loadFile(sFileToLoad, true, true, remapXrefs, decompileCodeIfNoPdb);
        }

        public void loadFile(string sFileToLoad, bool showNotSupportedExtensionError, bool useCachedVersionIfAvailable, bool remapXrefs, bool decompileCodeIfNoPdb)
        {
            CirDataAnalysisUtils.loadFileIntoCirDataAnalysisObject(sFileToLoad, cirDataAnalysis, showNotSupportedExtensionError, useCachedVersionIfAvailable, remapXrefs, decompileCodeIfNoPdb);   // load file           
            updateCirDataStats();
        }

        public void loadFiles(IEnumerable<string> filesToLoad, bool remapXrefs, bool decompileCodeIfNoPdb)
        {
            loadFiles(filesToLoad, true, true, remapXrefs, decompileCodeIfNoPdb);
        }

        public void loadFiles(IEnumerable<string> filesToLoad, bool showNotSupportedExtensionError, bool useCachedVersionIfAvailable, bool remapXrefs, bool decompileCodeIfNoPdb)
        {
            foreach (var fileToLoad in filesToLoad)
                CirDataAnalysisUtils.loadFileIntoCirDataAnalysisObject(fileToLoad, cirDataAnalysis, showNotSupportedExtensionError, useCachedVersionIfAvailable, false /*runRemapXrefs*/, decompileCodeIfNoPdb); // since we are loading multiple files never run the remap on file load

            if (remapXrefs)
                CirDataAnalysisUtils.remapXrefs(cirDataAnalysis);

            updateCirDataStats();
        }

        public ascx_FunctionsViewer getFunctionsViewerControl()
        {
            return functionsViewer;
        }

        private void showFunctionInformationOnNewWindow(object selectedItem)
        {
            var cirFunction = getCirFunctionFromSelectedItem(selectedItem);
            if (cirFunction != null)
                ascx_FunctionCalls.viewCirFunctionSignatureOnNewForm(cirFunction);
        }

        private void showFunctionInformation(object selectedItem)
        {
            var cirFunction = getCirFunctionFromSelectedItem(selectedItem);
            if (cirFunction != null)
            {
                if (gbCirTrace.Visible)
                    cirTraceForSelectedItem.viewCirFunction(cirFunction);
                if (gbSelectedItemInfo.Visible)
                    functionCallsForSelectedItem.viewCirFunction(cirFunction);
            }
            else
            {
                var cirClass = getCirClassFromSelectedItem(selectedItem);
                if (cirClass != null)
                    functionCallsForSelectedItem.viewCirClass(cirClass);
            }
        }

        private ICirClass getCirClassFromSelectedItem(object selectedItem)
        {
            if (selectedItem != null && (selectedItem is List<FilteredSignature>))
            {
                var filteredSignatures = (List<FilteredSignature>) selectedItem;
                foreach (var filteredSignature in filteredSignatures)
                {                    
                    var classSignature = filteredSignature.sFunctionClass;
                    if (cirDataAnalysis.dCirClass_bySignature.ContainsKey(classSignature))
                        return cirDataAnalysis.dCirClass_bySignature[classSignature];
                }
            }
            return null;
        }

        public ICirFunction getCirFunctionFromSelectedItem(object selectedItem)
        {
            if (selectedItem != null && selectedItem is FilteredSignature)
            {
                var filteredSignature = (FilteredSignature)selectedItem;
                var functionSignature = filteredSignature.sOriginalSignature;
                if (cirDataAnalysis.dCirFunction_bySignature.ContainsKey(functionSignature))
                    return cirDataAnalysis.dCirFunction_bySignature[functionSignature];
                // if we could not find using the originalSignature, try the signature
                foreach (var cirFunction in cirDataAnalysis.dCirFunction_bySignature.Values)
                {
                    var s = new FilteredSignature(cirFunction).sSignature;
                    var d = filteredSignature.sSignature;
                    if (new FilteredSignature(cirFunction).sSignature == filteredSignature.sSignature)
                    {
                        return cirFunction;
                    }
                    if (cirFunction.FunctionName == filteredSignature.sFunctionName)
                    { 
                    }
                }
            }
            return null;
        }

        public void createO2AssessmentWithCallFlowTraces()
        {
            O2Thread.mtaThread(
                () =>
                    {
                        this.invokeOnThread(() => btCreateO2AssessmentWithCallFlowTraces.Enabled = false);
                        var createdFile = CirTraces.createO2AssessmentWithCallFlowTraces(cirDataAnalysis);
                        if (File.Exists(createdFile))
                        {
                            ascx_FindingsViewer.o2AssessmentLoadEngines.Add(new O2AssessmentLoad_OunceV6());
                            ascx_FindingsViewer.o2AssessmentSave = new O2AssessmentSave_OunceV6();
                            ascx_FindingsViewer.openInFloatWindow(createdFile);
                        }
                        this.invokeOnThread(() => btCreateO2AssessmentWithCallFlowTraces.Enabled = true);                        
                    });
        }

        public void createO2AssessmentFromCirFunctions(List<ICirFunction> cirFunctions)
        {
            O2Thread.mtaThread(
                () =>
                {
                    this.invokeOnThread(() => btCreateO2AssessmentWithCallFlowTraces.Enabled = false);
                    var O2Findings = CirTraces.createO2FindingsFromCirFunctions(cirFunctions);
                    ascx_FindingsViewer.openInFloatWindow(O2Findings);                    
                    this.invokeOnThread(() => btCreateO2AssessmentWithCallFlowTraces.Enabled = true);
                });
        }


        private void createO2AssessmentWithFunctionsSignatures(List<FilteredSignature> filteredSignatures)
        {
            var cirFunctionsToProcess = new List<ICirFunction>();

            foreach(var filteredSignature in filteredSignatures)
                if (cirDataAnalysis.dCirFunction_bySignature.ContainsKey(filteredSignature.sOriginalSignature))
                    cirFunctionsToProcess.Add(cirDataAnalysis.dCirFunction_bySignature[filteredSignature.sOriginalSignature]);
            createO2AssessmentFromCirFunctions(cirFunctionsToProcess);
        }

        private void deleteAllLoadedData()
        {
            // clean current results  but keep the current viewing flags
            cirDataAnalysis =  new CirDataAnalysis
                                         {
                                             onlyShowExternalFunctionsThatAreInvokedFromCFG = cirDataAnalysis.onlyShowExternalFunctionsThatAreInvokedFromCFG,
                                             onlyShowFunctionsOrClassesWithControlFlowGraphs = cirDataAnalysis.onlyShowFunctionsOrClassesWithControlFlowGraphs,
                                             onlyShowFunctionsWithCallersOrCallees = cirDataAnalysis.onlyShowFunctionsWithCallersOrCallees
                                         };      
            showLoadedFunctions();
            updateCirDataStats();
        }

        private List<ICirFunction> getFunctionsListWith_LostSinks()
        {
            var matchedCirFunctions = new List<ICirFunction>();
            foreach (var cirFunction in cirDataAnalysis.dCirFunction_bySignature.Values)
                //if (cirFunction.FunctionsCalledUniqueList.Count == 0)
                if (cirFunction.HasControlFlowGraph == false)
                    matchedCirFunctions.Add(cirFunction);
            DI.log.info("There are {0} Lost Sinks", matchedCirFunctions.Count);
            return matchedCirFunctions;
        }

        private List<ICirFunction> getFunctionsListWith_LostSources()
        {
            var matchedCirFunctions = new List<ICirFunction>();
            foreach (var cirFunction in cirDataAnalysis.dCirFunction_bySignature.Values)
                                                                        // a lost source is a funtion that
                if (cirFunction.FunctionIsCalledBy.Count == 0           // is NOT called by another function                    
                    && cirFunction.FunctionsCalledUniqueList.Count >0     // calls at least one other function
                    && cirFunction.FunctionParameters.Count >0)         // has at least one parameter
                    matchedCirFunctions.Add(cirFunction);
            DI.log.info("There are {0} Lost Sources", matchedCirFunctions.Count);
            return matchedCirFunctions;
        }

        private List<ICirFunction> getFunctionsListWith_CurrentFilteredFunctions()
        {
            var currentFilteredFuntions = functionsViewer.lsSignatures.ToList();
            return cirDataAnalysis.CirFunctions<ICirFunction>();            
        }

        private List<ICirFunction>  getAllFunctions()
        {
            var allCirFunctions = new List<ICirFunction>();
            allCirFunctions.AddRange(cirDataAnalysis.dCirFunction_bySignature.Values);
            return allCirFunctions;
        }

        private void handleDrop(object droppedObject, bool remapXRefs, bool decompileCodeIfNoPdb)
        {
            this.invokeOnThread(() =>
                {
                    lbLoadingDroppedFile.Visible = true;
                });

            var fileOrDirectoryToLoad = droppedObject.ToString();
            if (droppedObject is Assembly)
                loadFile(((Assembly)droppedObject).Location, remapXRefs, decompileCodeIfNoPdb);
            else if (droppedObject is List<FilteredSignature>)
                showSignatures(((List<FilteredSignature>)droppedObject));
            else if (File.Exists(fileOrDirectoryToLoad))
                loadFile(fileOrDirectoryToLoad, remapXRefs, decompileCodeIfNoPdb);
            else if (Directory.Exists(fileOrDirectoryToLoad))
            {
                loadFiles(Directory.GetFiles(fileOrDirectoryToLoad), remapXRefs, decompileCodeIfNoPdb);
            }
            else if (droppedObject is List<ICirFunction>)
            {
                addCirFunctions((List<ICirFunction>)droppedObject);
            }
            else if (droppedObject is List<MethodInfo>)
                addMethodInfos((List<MethodInfo>) droppedObject);
            this.invokeOnThread(() => lbLoadingDroppedFile.Visible = false);
        }

        private void addMethodInfos(List<MethodInfo> methodInfos)
        {            
            var cirFunctions = new List<ICirFunction>();
            var cirDataAnalysis = new CirFactory().createCirDataAnalysisObject(methodInfos);
            addCirFunctions(cirDataAnalysis.dCirFunction_bySignature.Values.ToList());
                                         
        }

        private void addCirFunctions(List<ICirFunction> cirFunctionsToAdd)
        {
            foreach (var cirFunction in cirFunctionsToAdd)
                //if (false == cirDataAnalysis.dCirFunction_bySignature.ContainsValue(cirFunction))
                if (false == cirDataAnalysis.dCirFunction_bySignature.ContainsKey(cirFunction.FunctionSignature))
                    //if (false == cirDataAnalysis.dCirFunction_bySignature.ContainsKey[cirFunction.FunctionSignature]);
                        cirDataAnalysis.dCirFunction_bySignature.Add(cirFunction.FunctionSignature, cirFunction);
            updateCirDataStats(false);

            var signaturesToShow = new List<string>();
            foreach (var cirFunctionToAdd in cirFunctionsToAdd)
                signaturesToShow.Add(cirFunctionToAdd.FunctionSignature);
            showSignatures(signaturesToShow);
        }

        /*public void loadO2ObjectModel()
        {
            O2Thread.mtaThread(
                () =>
                    {
                        this.invokeOnThread(() => lbLoadingDroppedFile.Visible = true);
                        var assembliesToLoad = CompileEngine.getListOfO2AssembliesInExecutionDir();
                        loadFiles(assembliesToLoad);
                        this.invokeOnThread(() => lbLoadingDroppedFile.Visible = false);
                    });
        }*/

        private void handleOnItemDrag(object objectToDrag)
        {
            if (objectToDrag is FilteredSignature)
                objectToDrag = new List<FilteredSignature> { (FilteredSignature)objectToDrag };
            if (objectToDrag is List<FilteredSignature>)
            {
                var cirFunctions = new List<ICirFunction>();
                foreach(var functionSignature in (List<FilteredSignature>)objectToDrag)
                {
                    var cirFunction = mapFilteredSignatureToCirFunction(functionSignature);
                    if (cirFunction != null)
                    {
                        cirFunctions.Add(cirFunction);
                    }
                    /*if (cirDataAnalysis.dCirFunction_bySignature.ContainsKey(functionSignature.sSignature))
                        cirFunctions.Add(cirDataAnalysis.dCirFunction_bySignature[functionSignature.sSignature]);
                    else
                        DI.log.error("in handleOnItemDrag could not resolve into CirFunction the signature: {0}", functionSignature.sSignature);
                     * */
                }

                objectToDrag = cirFunctions;

            }           
            functionsViewer.doTreeViewDragDrop(objectToDrag);
                
        }

        public ICirDataAnalysis loadCirData(ICirData cirDataToLoad)
        {
            return loadCirData(cirDataToLoad, true);
        }

        public ICirDataAnalysis loadCirData(ICirData cirDataToLoad, bool clearLoadedData)
        {
            if (clearLoadedData)
                deleteAllLoadedData();
            CirDataAnalysisUtils.addO2CirDataFile(cirDataAnalysis, cirDataToLoad);
            return cirDataAnalysis;
        }

        private void saveAsCirDataFile()
        {
            var savedCirDataFile = DI.config.getTempFileInTempDirectory("CirData");
            if (cirDataAnalysis.Save(savedCirDataFile))
            {
                var userMessage = string.Format("new CirData file Created: {0}", savedCirDataFile);
                DI.log.info(userMessage);
                DI.log.showMessageBox(userMessage);
            }
        }

        public ICirData getCirDataObject()
        {            
            return CirDataAnalysisUtils.createCirDataFromCirDataAnalysis(cirDataAnalysis);
        }
    }
}
