import { CreateModelDialog, ModelListTable } from "@/features/modelList";

const Models = () => {
  return (
    <div className="grid items-center justify-center space-y-5 pt-10">
      <div className="flex w-full justify-end">
        <CreateModelDialog />
      </div>
      <ModelListTable className="max-w-3xl" />
    </div>
  );
};

export default Models;
