import MakeSelect from "@/components/forms/select/MakeSelect";
import ModelSelect from "@/components/forms/select/ModelSelect";
import { VariantListTable } from "@/features/variantList";
import { useState } from "react";

const Variants = () => {
  const [selectedMake, setSelectedMake] = useState<string | undefined>();
  const [selectedModel, setSelectedModel] = useState<string | undefined>();

  const handleMakeChange = (value: string) => {
    setSelectedMake(value);
    setSelectedModel(undefined);
  };

  return (
    <div className="grid items-center justify-center space-y-5 pt-10">
      <div className="flex w-full gap-4">
        <MakeSelect
          isAllMakesEnabled={false}
          label="Make"
          value={selectedMake ?? ""}
          onValueChange={handleMakeChange}
          className="w-48"
        />
        <ModelSelect
          isAllModelsEnabled={false}
          label="Model"
          selectedMake={selectedMake}
          value={selectedModel ?? ""}
          onValueChange={setSelectedModel}
          className="w-48"
        />
      </div>
      {selectedModel && (
        <VariantListTable modelId={selectedModel} className="max-w-4xl" />
      )}
    </div>
  );
};

export default Variants;
